using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using AKK.Classes.Models;
using AKK.Classes.ApiResponses;
using AKK.Classes.Models.Repository;
using AKK.Services;

namespace AKK.Controllers {
    [Route("api/section")]
    public class SectionController : Controller {
        IRepository<Section> _sectionRepository;
        IAuthenticationService _authenticationService;
        public SectionController(IRepository<Section> sectionRepository, IAuthenticationService authenticationService)
        {
            _sectionRepository = sectionRepository;
            _authenticationService = authenticationService;
        }

        // GET: /api/section
        [HttpGet]
        public ApiResponse<IEnumerable<Section>> GetAllSections() {
            var sections = _sectionRepository.GetAll().OrderBy(s => s.Name);

            return new ApiSuccessResponse<IEnumerable<Section>>(sections);
        }

        // POST: /api/section
        [HttpPost]
        public ApiResponse<Section> AddSection(string token, string name) {
            if (!_authenticationService.HasRole(token, Role.Admin))
            {
                return new ApiErrorResponse<Section>("You need to be logged in as an administrator to add a new section");
            }
            var sectionExsits = _sectionRepository.GetAll().Where(s => s.Name == name);
            if(sectionExsits.Any()) {
                return new ApiErrorResponse<Section>("A section with name "+name+" already exist");
            }
            if (name == null)
            {
                return new ApiErrorResponse<Section>("Name must have a value");
            }
            Section section = new Section() {Name=name};
            _sectionRepository.Add(section);
            try
            {
                _sectionRepository.Save();
                return new ApiSuccessResponse<Section>(section);
            }
            catch
            {
                return new ApiErrorResponse<Section>("Failed to create new section with name " + name);
            }
        }

        // DELETE: /api/section
        [HttpDelete]
        public ApiResponse<IEnumerable<Section>> DeleteAllSections(string token) {
            if (!_authenticationService.HasRole(token, Role.Admin))
            {
                return new ApiErrorResponse<IEnumerable<Section>>("You need to be logged in as an administrator to delete all sections");
            }
            var sections = _sectionRepository.GetAll();
            if(!sections.Any())
                return new ApiErrorResponse<IEnumerable<Section>>("No sections exist");
            
            // create copy that can be sent as result
            var resultCopy = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(sections)) as IEnumerable<Section>;

            foreach (Section section in sections)
            {
                _sectionRepository.Delete(section);
            }

            try
            {
                _sectionRepository.Save();
                return new ApiSuccessResponse<IEnumerable<Section>>(resultCopy);
            }
            catch
            {
                return new ApiErrorResponse<IEnumerable<Section>>("Failed to remove sections from database");
            }
        }

        // GET: /api/section/{name}
        [HttpGet("{name}")]
        public ApiResponse<Section> GetSection(string name) {
            var sections = _sectionRepository.GetAll();
            
            try {
                Guid id = new Guid(name);
                sections = sections.Where(s => s.Id == id);
            } catch(System.FormatException) {
                sections = sections.Where(s => s.Name == name);
            }

            if(sections.Count() != 1)
                return new ApiErrorResponse<Section>("No section with name/id " + name);

            return new ApiSuccessResponse<Section>(sections.First());
        }

        // DELETE: /api/section/{name}
        [HttpDelete("{name}")]
        public ApiResponse<Section> DeleteSection(string token, string name) {
            if (!_authenticationService.HasRole(token, Role.Admin))
            {
                return new ApiErrorResponse<Section>("You need to be logged in as an administrator to delete this section");
            }
            Section section;
            
            try {
                Guid id = new Guid(name);
                section = _sectionRepository.Find(id);
            } catch(System.FormatException) {
                section = _sectionRepository.GetAll().FirstOrDefault(s => s.Name == name);
            }

            if(section == null)
                return new ApiErrorResponse<Section>("No section exists with name/id "+name);
            else {
                // create copy that can be sent as result // we dont map so that we can output the deleted routes as well
                var resultCopy = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(section)) as Section;
        
                _sectionRepository.Delete(section);

                try
                {
                    _sectionRepository.Save();
                    return new ApiSuccessResponse<Section>(resultCopy);
                }
                catch
                {
                    return new ApiErrorResponse<Section>("Failed to delete section with name/id " + name);
                }
            }

        }

        // GET: /api/section/{name}/routes
        [HttpGet("{name}/routes")]
        public ApiResponse<IEnumerable<Route>> GetSectionRoutes(string name)
        {
            Section section;

            try {
                Guid id = new Guid(name);
                section = _sectionRepository.Find(id);
            } catch(System.FormatException) {
                section = _sectionRepository.GetAll().FirstOrDefault(s => s.Name== name);
            }

            if(section == null)
                return new ApiErrorResponse<IEnumerable<Route>>("No section with name/id "+name);
            return new ApiSuccessResponse<IEnumerable<Route>>(section.Routes);
        }

        // DELETE: /api/section/{name}/routes
        [HttpDelete("{name}/routes")]
        public ApiResponse<IEnumerable<Route>> DeleteSectionRoutes(string token, string name)
        {
            if (!_authenticationService.HasRole(token, Role.Admin))
            {
                return new ApiErrorResponse<IEnumerable<Route>>("You need to be logged in as an administrator to delete section routes");
            }
            Section section;

            try
            {
                Guid id = new Guid(name);
                section = _sectionRepository.Find(id);
            }
            catch (System.FormatException)
            {
                section = _sectionRepository.GetAll().FirstOrDefault(s => s.Name == name);
            }

            if (section == null)
                return new ApiErrorResponse<IEnumerable<Route>>("No section with name/id "+name);
            
            // create copy that can be sent as result
            var resultCopy = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(section.Routes)) as IEnumerable<Route>;
            section.Routes.RemoveAll(r => true);

            try
            {
                _sectionRepository.Save();
                return new ApiSuccessResponse<IEnumerable<Route>>(resultCopy);

            }
            catch
            {
                return new ApiErrorResponse<IEnumerable<Route>>("Failed to delete routes of section with name/id " + name);
            }
        }
    }
}