using AKK.Controllers;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.InteropServices.ComTypes;
using AKK.Controllers.ApiResponses;
using AKK.Models;
using AKK.Models.Repositories;
using AKK.Services;
using Microsoft.AspNetCore.Http;

namespace AKK.Tests.Controllers
{
    [TestFixture]
    public class RouteControllerTests
    {
        private TestDataFactory _dataFactory;
        private RouteController _controller;
        private IRepository<Section> _sectionRepo;
        private IRepository<Grade> _gradeRepo;
        private IRepository<Route> _routeRepo;
        private IRepository<Image> _imageRepo;
        private IRepository<Hold> _holdRepo;
        private IRepository<HoldColor> _holdColorRepo;
        private IRepository<Member> _memberRepo;
        private IAuthenticationService _auth;

        private string token;
        private Route testRoute;

        [OneTimeSetUp] // Runs once before first test
        public void SetUpSuite() { }

        [OneTimeTearDown] // Runs once after last test
        public void TearDownSuite() { }

        [SetUp] // Runs before each test
        public void SetupTest () 
        { 
            _dataFactory = new TestDataFactory();
            _sectionRepo = new TestRepository<Section>(_dataFactory.Sections);
            _gradeRepo = new TestRepository<Grade>(_dataFactory.Grades);
            _routeRepo = new TestRepository<Route>(_dataFactory.Routes);
            _imageRepo = new TestRepository<Image>(_dataFactory._images);
            _holdRepo = new TestRepository<Hold>(_dataFactory._holds);
            _holdColorRepo = new TestRepository<HoldColor>(_dataFactory._holdColors);

            _memberRepo = new TestRepository<Member>();
            _auth = new AuthenticationService(_memberRepo);
            _memberRepo.Add(new Member {Id = new Guid(), DisplayName = "TannerHelland", Username = "Tanner", Password = _auth.HashPassword("Helland"), IsAdmin = false, Token = "TannerHelland"});
            _memberRepo.Add(new Member {Id = new Guid(), DisplayName = "Morten Rask", Username = "Morten", Password = _auth.HashPassword("Rask"), IsAdmin = true, Token = "AdminTestToken"});
            _controller = new RouteController(_routeRepo, _sectionRepo, _gradeRepo, _imageRepo, _holdRepo, _holdColorRepo, _memberRepo, _auth);
            
            testRoute = new Route();
            Route temp = _routeRepo.GetAll().First();
            testRoute.GradeId = temp.GradeId;
            testRoute.SectionId = temp.SectionId;
            testRoute.Name = temp.Name;
            testRoute.Member = temp.Member;
            testRoute.Author = temp.Author;
            testRoute.ColorOfHolds = temp.ColorOfHolds;

            token = _auth.Login("Morten", "Rask");
        }

        [TearDown] // Runs after each test
        public void TearDownTest() 
        {
            _dataFactory = null;
            _sectionRepo = null;
            _gradeRepo = null;
            _routeRepo = null;
            _imageRepo = null;
            _holdRepo = null;
            _holdColorRepo = null;
            _memberRepo = null;
            _auth = null;
            _controller = null;
            testRoute = null;
            token = null;
        }

        [Test]
        public void _GetRoutes_GettingAllRoutesInTheSystem_ExpectTheyreAllThere()
        {
            var response = _controller.GetRoutes(null,null,null,0,SortOrder.Newest);
            var routes = response.Data;
            Assert.AreEqual(true, response.Success);
            
            CollectionAssert.AreEquivalent(_routeRepo.GetAll(), routes);
        }

        [Test]
        public void _GetRoutes_GettingRoutesOfCertainGrade_ExpectOnlyRoutesWithThatGrade()
        {
            var grade = _gradeRepo.GetAll().First(g => g.Name == "Green");
            var response = _controller.GetRoutes(grade.Id, null, null, 0, SortOrder.Newest);
            var routes = response.Data;

            Assert.AreEqual(true, response.Success);

            foreach (var route in routes)
            {
                if (route.Grade.Name != "Green")
                {
                    Assert.Fail($"  Expected: Route with Grade Green\n  But Was: {route.Grade.Name}");
                }
            }
            
            Assert.Pass();
        }

        [Test]
        public void _GetRoutes_Getting5Routes_ExpectOnly5Routes()
        {
            var response = _controller.GetRoutes(null,null,null,5,SortOrder.Newest);
            var routes = response.Data;

            Assert.AreEqual(true, response.Success);

            Assert.AreEqual(5, routes.Count());
        }

        [Test]
        public void _GetRoutes_GettingRoutesFromSectionWithID_ExpectOnlyRoutesFromThatSection()
        {
            var section = _sectionRepo.GetAll().First();
            var response = _controller.GetRoutes(null, section.Id, null, 0, SortOrder.Newest);
            var routes = response.Data;

            Assert.IsTrue(response.Success);

            CollectionAssert.AreEquivalent(section.Routes, routes);
        }

        [Test]
        public void _GetRoutes_GettingAllRoutesWithSortOrderOfOldest_ExpectOldestRoutesFirst()
        {
            var response = _controller.GetRoutes(null,null, null, 0, SortOrder.Oldest);
            Assert.IsTrue(response.Success);

            var routes = response.Data.ToArray();
            int length = routes.Count();
            for (int i = 1; i < length; i++)
            {
                Assert.IsTrue(routes[i].CreatedDate.Subtract(routes[i-1].CreatedDate).TotalSeconds >= 0);
            }
        }

        [Test]
        public void _GetRoutes_GettingAllRoutesWithSortOrderOfGrades_ExpectLowestGradesFirst()
        {
            var response = _controller.GetRoutes(null,null, null, 0, SortOrder.Grading);
            Assert.IsTrue(response.Success);

            var routes = response.Data.ToArray();
            int length = routes.Count();
            for (int i = 1; i < length; i++)
            {
                Assert.IsTrue(routes[i-1].Grade.Difficulty <= routes[i].Grade.Difficulty);
            }
        }

        [Test]
        public void _GetRoutes_GettingAllRoutesWithSortOrderOfRating_ExpectHighestRatedRoutesFirst()
        {
            var response = _controller.GetRoutes(null,null, null, 0, SortOrder.Rating);
            Assert.IsTrue(response.Success);

            var routes = response.Data.ToArray();
            int length = routes.Count();
            for (int i = 1; i < length; i++)
            {
                Assert.IsTrue(routes[i-1].AverageRating >= routes[i].AverageRating);
            }
        }

        [Test]
        public void _AddRoute_NewRouteGetsAdded_RouteGetsAdded()
        {
            testRoute.Name = "50";
            var response = _controller.AddRoute(token, testRoute);

            Assert.True(response.Success, (response as ApiErrorResponse<Route>)?.ErrorMessage + "\n" + testRoute.SectionId);

            Assert.True(_dataFactory.Routes.AsEnumerable<Route>().Contains(testRoute));
        }

        [Test]
        public void _AddRoute_NewRouteWithBadGradeId_RouteDoesntGetAdded()
        {
            testRoute.GradeId = new Guid();
            var response = _controller.AddRoute(token, testRoute);

            Assert.False(response.Success);
        }

        [Test]
        public void _AddRoute_NewRouteWithBadSectionId_RouteDoesntGetAdded()
        {
            testRoute.SectionId = new Guid();
            var response = _controller.AddRoute(token, testRoute);
            
            Assert.False(response.Success);
        }

        [Test]
        public void _AddRoute_NewRouteWithAnExistingID_RouteGetsAdded()
        {
            testRoute.SectionId = _sectionRepo.GetAll().ElementAt(1).Id;
            testRoute.Name = "15";
            var response = _controller.AddRoute(token, testRoute);

            Assert.True(response.Success);
        }

        [Test]
        public void _AddRoute_NewRouteWithAnExistingNameAndGrade_RouteDoesntGetAdded()
        {
            testRoute.Name = _routeRepo.GetAll().First().Name;
            testRoute.Grade = _routeRepo.GetAll().First().Grade;
            testRoute.GradeId = _routeRepo.GetAll().First().GradeId;

            var response = _controller.AddRoute(token, testRoute);

            Assert.False(response.Success);
        }

        [Test]
        public void _DeleteAllRoutes_AdminDeletesAllRoutes_AllRoutesGetDeleted()
        {
            var response = _controller.DeleteAllRoutes(token);

            Assert.True(response.Success);
            Assert.AreEqual(0, _routeRepo.GetAll().Count());
        }

        [Test]
        public void _DeleteAllRoutes_MemberDeletesAllRoutes_NoRoutesGetDeleted()
        {
            var token = _auth.Login("Tanner", "Helland");
            var response = _controller.DeleteAllRoutes(token);

            Assert.False(response.Success);
        }

        [Test]
        public void _DeleteAllRoutes_GuestDeletesAllRoutes_NoRoutesGetDeleted()
        {
            var response = _controller.DeleteAllRoutes("123");

            Assert.False(response.Success);
        }

        [Test]
        public void _GetRoute_GetExistingRouteByItsID_RouteReturned()
        {
            testRoute = _routeRepo.GetAll().First();
            var response = _controller.GetRoute(testRoute.Id);

            Assert.True(response.Success);
            Assert.AreEqual(testRoute, response.Data);
        }

        [Test]
        public void _GetRoute_GetNonExistingRoute_NoRouteFound()
        {
            var response = _controller.GetRoute(testRoute.Id);

            Assert.False(response.Success);
        }

        [Test]
        public void _GetImage_GetExistingImageByItsID_ImageReturned()
        {
            var response = _controller.GetImage(_routeRepo.GetAll().First().Id);

            Assert.True(response.Success);
        }

        [Test]
        public void _GetImage_GetNonExistingImageByID_NoImageReturned()
        {
            Image test = new Image();

            var response = _controller.GetImage(test.Id);

            Assert.False(response.Success);
        }
/*
        [Test]
        public void _AddBeta_AddBetaToRouteAsMember_BetaGetsAdded()
        {
            IFormFile beta;
            beta.OpenReadStream();
            var response = _controller.AddBeta(token, beta, _routeRepo.GetAll().FirstOrDefault().Id, "");
            Assert.IsTrue(response.Result.Success);
            Assert.IsNotEmpty(_routeRepo.GetAll().First().Videos);
        }
*/
        [Test]
        public void _UpdateRoute_UpdateNameOnRoute_NameGetsUpdated()
        {
            Route routeToUpdate = _routeRepo.GetAll().First();
            testRoute.Name = "40";

            var response = _controller.UpdateRoute(token, routeToUpdate.Id, testRoute);

            Assert.True(response.Success);

            Assert.AreEqual("40", response.Data.Name);
            Assert.AreEqual("40", _routeRepo.Find(routeToUpdate.Id).Name);
        }

        [Test]
        public void _UpdateRoute_UpdateGradeIdOnRoute_GradeGetsUpdated()
        {
            Route routeToUpdate = _routeRepo.GetAll().First();
            testRoute.GradeId = _gradeRepo.GetAll().First(g => g.Difficulty == 3).Id;

            var response = _controller.UpdateRoute(token, routeToUpdate.Id, testRoute);

            Assert.True(response.Success);
            Assert.AreEqual(response.Data.GradeId, testRoute.GradeId);
            Assert.AreEqual(_routeRepo.Find(routeToUpdate.Id).GradeId, testRoute.GradeId);
        }

        [Test]
        public void _UpdateRoute_UpdateNameAndId_NameGetsUpdatedAndIdDoesNotUpdate() 
        {
            Route routeToUpdate = _routeRepo.GetAll().First();
            
            Guid oldId = routeToUpdate.Id;
            testRoute.Name = "40";
            testRoute.Id = Guid.NewGuid();

            Assert.NotNull(_routeRepo.Find(oldId));

            var response = _controller.UpdateRoute(token, routeToUpdate.Id, testRoute);

            Assert.True(response.Success);

            Assert.AreEqual("40", response.Data.Name);
            Assert.AreEqual(oldId, response.Data.Id);
            Assert.AreEqual("40", _routeRepo.Find(oldId).Name);
        }

        [Test]
        public void _UpdateRoute_UpdateSectionId_SectionIdGetsUpdated() 
        {
            Route routeToUpdate = _routeRepo.GetAll().First();
            testRoute.SectionId = _routeRepo.GetAll().First(s => s.SectionId != routeToUpdate.SectionId).SectionId;

            var response = _controller.UpdateRoute(token, routeToUpdate.Id, testRoute);

            Assert.True(response.Success);
            Assert.AreEqual(routeToUpdate.SectionId, response.Data.SectionId);
            Assert.AreEqual(routeToUpdate.SectionId, _routeRepo.Find(routeToUpdate.Id).SectionId);
        }

        [Test]
        public void _UpdateRoute_UpdateSectionIdAndGradeId_SectionIdAndGradeIdGetsUpdated() 
        {
            Route routeToUpdate = _routeRepo.GetAll().First();
            Guid gradeId = _gradeRepo.GetAll().First(g => g.Difficulty == 3).Id;
            Guid sectionId = _routeRepo.GetAll().First(s => s.Section.Name == "C").SectionId;
            testRoute.GradeId = gradeId;
            testRoute.SectionId = sectionId;

            var response = _controller.UpdateRoute(token, routeToUpdate.Id, testRoute);

            Assert.True(response.Success);
            Assert.AreEqual(response.Data.GradeId, gradeId);
            Assert.AreEqual(_routeRepo.Find(routeToUpdate.Id).GradeId, gradeId);
            Assert.AreEqual(routeToUpdate.SectionId, response.Data.SectionId);
            Assert.AreEqual(routeToUpdate.SectionId, _routeRepo.Find(routeToUpdate.Id).SectionId);
        }

        [Test]
        public void _UpdateRoute_UpdateRouteNumberAndGradeToRouteThatExistsWithThoseValues_RouteDoesntGetUpdated() 
        {
            Route routeToUpdate = _routeRepo.GetAll().First();
            testRoute.Name = _routeRepo.GetAll().ElementAt(1).Name;
            testRoute.Grade = _routeRepo.GetAll().ElementAt(1).Grade;
            testRoute.GradeId = _routeRepo.GetAll().ElementAt(1).GradeId;
            

            var response = _controller.UpdateRoute(token, routeToUpdate.Id, testRoute);

            Assert.False(response.Success);
        }

        [Test]
        public void _UpdateRoute_UpdateHoldOnRoute_HoldGetsUpdated()
        {
            Route Origroute = _routeRepo.GetAll().First();
            testRoute.ColorOfHolds = _holdColorRepo.GetAll().ElementAt(4).ColorOfHolds;

            var response = _controller.UpdateRoute(token, Origroute.Id, testRoute);

            Assert.AreEqual(testRoute.ColorOfHolds.R, Origroute.ColorOfHolds.R);
        }

        [Test]
        public void _UpdateRoute_AddTapeOnRouteWithNoTape_TapeGetsAdded()
        {
            Route Origroute = _routeRepo.GetAll().First();
            testRoute.ColorOfTape = _holdColorRepo.GetAll().First().ColorOfHolds;
            var response = _controller.UpdateRoute(token, Origroute.Id, testRoute);

            Assert.True(testRoute.ColorOfTape.Equals(Origroute.ColorOfTape));
        }

        [Test]
        public void _UpdateRoute_UpdateTapeOnRouteWithTape_TapeGetsUpdated()
        {
            Route Origroute = _routeRepo.GetAll().First();
            Color temp = _routeRepo.GetAll().First(t => t.Author == "Manfred").ColorOfTape;

            Origroute.ColorOfTape = temp;
            testRoute.ColorOfTape = temp;
            testRoute.ColorOfTape.R += 1;

            var response = _controller.UpdateRoute(token, Origroute.Id, testRoute);

            Assert.AreEqual(testRoute.ColorOfTape.R, Origroute.ColorOfTape.R);
        }

        [Test]
        public void _UpdateRoute_UpdateTapeOnRouteRemoveTape_TapeGetsRemoved()
        {
            Route Origroute = _routeRepo.GetAll().First(t => t.Author == "Manfred");
            Route test = new Route();

            test.Author = Origroute.Author;
            test.ColorOfHolds = Origroute.ColorOfHolds;
            test.ColorOfTape = null;
            test.CreatedDate = Origroute.CreatedDate;
            test.Grade = Origroute.Grade;
            test.GradeId = Origroute.GradeId;
            test.HexColorOfHolds = Origroute.HexColorOfHolds;
            test.HexColorOfTape = null;

            var response = _controller.UpdateRoute(token, Origroute.Id, test);

            Assert.Null(Origroute.ColorOfTape);
        }

        [Test]
        public void _UpdateRoute_UpdateUserWhileNotAuthenticated_ErrorResponseAndRouteNotUpdated()
        {
            Route Origroute = _routeRepo.GetAll().First();
            
            var response = _controller.UpdateRoute("123", Origroute.Id, testRoute);

            Assert.False(response.Success);
            Assert.AreNotEqual(testRoute, Origroute);
        }

        [Test]
        public void _DeleteRoute_DeleteExistingRoute_RouteGetsDeleted()
        {
            Route Origroute = _routeRepo.GetAll().First();
            int numberOfRoutes = _routeRepo.GetAll().Count();
            var response = _controller.DeleteRoute(token, Origroute.Id);

            Assert.True(response.Success);
            Assert.AreEqual(numberOfRoutes - 1, _routeRepo.GetAll().Count());
            Assert.AreNotEqual(Origroute.Author, _routeRepo.GetAll().First().Author);
        }

        [Test]
        public void _DeleteRoute_DeleteRouteThatDoesntExist_Error()
        {
            var response = _controller.DeleteRoute(token, new Guid());

            Assert.False(response.Success);
        }

        [Test]
        public void _DeleteRoute_DeleteRouteAsGuest_Error()
        {
            Route Origroute = _routeRepo.GetAll().First();
            var response = _controller.DeleteRoute("123", Origroute.Id);

            Assert.False(response.Success);
        }

        [Test]
        public void _DeleteRoute_DeleteRouteWithIdOfSection_Error()
        {
            var response = _controller.DeleteRoute(token, _sectionRepo.GetAll().First().Id);

            Assert.False(response.Success);
        }

        [Test]
        public void _SetRating_SetRatingAsMemberToRoute_RatingGetsAdded()
        {
            Route route = _routeRepo.GetAll().First();
            route.Ratings.RemoveAll(r => true);
            var response = _controller.SetRating(token, route.Id, 5);

            Assert.True(response.Success);
            Assert.AreEqual(1, route.Ratings.Count);
            Assert.AreEqual(5, route.Ratings.First().RatingValue);
        }

        [Test]
        public void _SetRating_SetRatingAsMemberToRouteWhichHasSeveralRatings_AverageIsCorrect()
        {
            Route route = _routeRepo.GetAll().First();
            route.Ratings.RemoveAll(r => true);
            _controller.SetRating(token, route.Id, 5);
            _controller.SetRating(_auth.Login("Tanner", "Helland"), route.Id, 3);
            
            Member testMember = new Member();
            
            testMember.Password = _auth.HashPassword("123");
            testMember.Username = "test";
            testMember.IsAdmin = false;
            testMember.DisplayName = "test123";
            _memberRepo.Add(testMember);
            var response = _controller.SetRating(_auth.Login("test", "123"), route.Id, 1);
            Assert.IsTrue(response.Success);

            Assert.AreEqual(3, route.Ratings.Count);
            Assert.AreEqual(3, route.AverageRating);
        }

        [Test]
        public void _SetRating_SetRatingAsGuest_NoRatingGetsAdded()
        {
            Route route = _routeRepo.GetAll().First();
            route.Ratings.RemoveAll(r => true);
            var response = _controller.SetRating("123", route.Id, 5);
            Assert.False(response.Success);
            Assert.IsEmpty(route.Ratings);
        }

        [Test]
        public void _SetRating_SetRatingAsMemberWhoAddedIt_RatingGetsUpdated()
        {
            Route route = _routeRepo.GetAll().First();
            route.Ratings.RemoveAll(r => true);
            var tokenForMember = _auth.Login("Tanner", "Helland");
            _controller.SetRating(tokenForMember, route.Id, 5);
            var response = _controller.SetRating(tokenForMember, route.Id, 2);

            Assert.True(response.Success);
            Assert.AreEqual(2, route.Ratings.First().RatingValue);
        }

        [Test]
        public void _SetRating_SetRatingAsGuest_RatingDoesntGetChanged()
        {
            Route route = _routeRepo.GetAll().First();
            route.Ratings.RemoveAll(r => true);

            _controller.SetRating(token, route.Id, 5);
            var response = _controller.SetRating("123", route.Id, 2);
    
            Assert.False(response.Success);
            Assert.AreEqual(5, route.Ratings.First().RatingValue);
        }
    }
}
