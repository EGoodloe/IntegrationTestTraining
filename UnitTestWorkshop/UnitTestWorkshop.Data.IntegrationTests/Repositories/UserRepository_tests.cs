using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using Moq;
using SMG.Core;
using UnitTestWorkshop.Data.Models.AccountModels;
using UnitTestWorkshop.Data.Models.QueryModels;
using UnitTestWorkshop.Data.Repositories;

namespace UnitTestWorkshop.Data.IntegrationTests.Repositories
{
    [TestClass]
    [Ignore]
    public class UserRepository_tests
    {
        private static UserRepository _subject;
        private static Mock<ISystemTime> _mockSystemTime;
        private static string _connectionString;
        private static MongoServer _server;
        private static MongoDatabase _database;
        private List<User> _testData;

        [ClassInitialize]
        [Description("Initializes only once per full test run.")]
        public static void InitializeRepo(TestContext context)
        {
            _mockSystemTime = new Mock<ISystemTime>();
            _subject = new UserRepository(_mockSystemTime.Object);

            _connectionString = ConfigurationManager.ConnectionStrings["TestingMongo"].ConnectionString;
            var client = new MongoClient(_connectionString);
            _server = client.GetServer();
            _database = _server.GetDatabase(new MongoUrl(_connectionString).DatabaseName);
        }

        [TestInitialize]
        [Description("Initializes before every Test.")]
        public void InitializeTestData()
        {
            _testData = new List<User>
            {
                new User
                {
                    CreationDate = new DateTime(1999,10,30),
                    Email = "JimBob@OMGDonuts.com",
                    FirstName = "Jim",
                    LastName = "Bob",
                    LastLogin = new DateTime(2015,12,2)
                },
                new User
                {
                    CreationDate = new DateTime(1974,04,28),
                    Email = "JaneBob@OMGCarrots.com",
                    FirstName = "Jane",
                    LastName = "Bob",
                    LastLogin = new DateTime(2015,12,1)
                }
            };

            var collection = _database.GetCollection<User>("users");
            foreach (var user in _testData)
            {
                collection.Insert(user);
            }
        }

        [TestMethod]
        public void When_Retrieving_Single_User_by_Email_Return_User()
        {
            //Arrange
            var expected = _testData.First();
            
            var input = new ByUserEmail
            {
                UserEmail = expected.Email
            };

            User actual;

            //Act
            actual = _subject.Retrieve(input);

            //Assert
            Assert.AreEqual(expected.CreationDate, actual.CreationDate, string.Format("Expected:{0} for CreationDate, Actual was {1}", expected.CreationDate, actual.CreationDate));
            Assert.AreEqual(expected.Email, actual.Email, string.Format("Expected:{0} for Email, Actual was {1}", expected.Email, actual.Email));
            Assert.AreEqual(expected.FirstName, actual.FirstName, string.Format("Expected:{0} for FirstName, Actual was {1}", expected.FirstName, actual.FirstName));
            Assert.AreEqual(expected.LastName, actual.LastName, string.Format("Expected:{0} for LastName, Actual was {1}", expected.LastName, actual.LastName));
            Assert.AreEqual(expected.LastLogin, actual.LastLogin, string.Format("Expected:{0} for LastLogin, Actual was {1}", expected.LastLogin, actual.LastLogin));
            Assert.AreEqual(expected.UserId, actual.UserId, string.Format("Expected:{0} for UserId, Actual was {1}", expected.UserId, actual.UserId));

        }

        [TestCleanup]
        [Description("Runs after each test has been run.")]
        public void CleanupTestData()
        {
            _server.DropDatabase(new MongoUrl(_connectionString).DatabaseName);
        }
    }
}
