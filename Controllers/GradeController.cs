using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using AKK.Classes.Models;
using AKK.Classes.ApiResponses;
using AKK.Classes.Models.Repository;


namespace AKK.Controllers {
    [Route("api/grade")]
    public class GradeController : Controller {
        IRepository<Grade> _gradeRepository;
        public GradeController(IRepository<Grade> gradeRepository)
        {
            _gradeRepository = gradeRepository;
        }

        // GET: /api/grade
        [HttpGet]
        public ApiResponse GetAllGrades() {
            var grades = _gradeRepository.GetAll().AsQueryable().OrderBy(g => g.Difficulty);

            return new ApiSuccessResponse(grades);
        }

        // POST: /api/grade
        [HttpPost]
        public ApiResponse AddGrade(Grade grade) {
            if(_gradeRepository.GetAll().Count(g => g.Difficulty == grade.Difficulty) != 0)
                return new ApiErrorResponse("A grade already exists with the given difficulty");

            _gradeRepository.Add(grade);
            try
            {
                _gradeRepository.Save();
                return new ApiSuccessResponse(grade);
            }
            catch
            {
                return new ApiErrorResponse("Failed to add grade");
            }
        }

        // GET: /api/grade/{id}
        [HttpGet("{id}")]
        public ApiResponse GetGrade(string id)
        {

            Grade grade = FindGrade(id);
            if(grade == null)
                return new ApiErrorResponse("No grades with given id exist");

            return new ApiSuccessResponse(grade);
        }

        // PATCH: /api/grade/{id}
        [HttpPatch("{id}")]
        public ApiResponse UpdateGrade(string id, int? difficulty, Color color) {
            Grade oldGrade = FindGrade(id);
            if(oldGrade == null)
                return new ApiErrorResponse("No grade exists with difficulty/id " + id);

            if(difficulty != null) {
                if(_gradeRepository.GetAll().Count(g => g.Difficulty == difficulty) > 0)
                    return new ApiErrorResponse("A grade with this difficulty already exist");
    
                oldGrade.Difficulty = (int)difficulty; 
            }

            if(color != null)
                oldGrade.Color = color;

            try
            {
                _gradeRepository.Save();
                return new ApiSuccessResponse(oldGrade);
            }
            catch
            {
                return new ApiErrorResponse("Failed to update grade to database");
            }
        }

        // DELETE: /api/grade/{id}
        [HttpDelete("{id}")]
        public ApiResponse DeleteGrade(string id) {
            Grade grade = FindGrade(id);
            if(grade == null)
                return new ApiErrorResponse("No grade exists with difficulty/id " + id);
            
            if(grade.Routes.Count() != 0)
                return new ApiErrorResponse("Routes already exists with this grade. Remove those before you delete this grade");

            // create copy that can be sent as result
            var resultCopy = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(grade));

            _gradeRepository.Delete(grade);

            try
            {
                _gradeRepository.Save();
                return new ApiSuccessResponse(resultCopy);

            }
            catch
            {
                return new ApiErrorResponse("Failed to remove grade from database");
            }
        }

        // GET: /api/grade/{id}/routes
        [HttpGet("{id}/routes")]
        public ApiResponse GetGradeRoutes(string id) {
            Grade grade = FindGrade(id);

            if(grade == null)
                return new ApiErrorResponse("No grades with given id exists");


            return new ApiSuccessResponse(grade.Routes);
        }

        // returns grade on either guid or difficulty
        public Grade FindGrade(string identifier) {
            // Guid guid = new Guid(identifier);
            int difficulty;
            Grade grade = null;
            if(int.TryParse(identifier, out difficulty)) {
                var grades = _gradeRepository.GetAll().Where(g => g.Difficulty == difficulty);
                if(grades.Count() == 1)
                    grade = grades.First();
            }
            /*
            else {
                var grades = queryContext.Where(g => g.Id == guid);
                if(grades.Count() == 1)
                    grade = grades.First();
            }
            */
            return grade;
        }
    }
}