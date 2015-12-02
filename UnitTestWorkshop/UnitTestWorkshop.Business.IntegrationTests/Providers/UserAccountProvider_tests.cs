using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SMG.Core;
using SMG.Core.Contracts;
using SMG.Core.Data.Contracts;
using UnitTestWorkshop.Business.Models.AccountModels;
using UnitTestWorkshop.Business.Providers;
using UnitTestWorkshop.Business.Translators;
using UnitTestWorkshop.Data.Models.AccountModels;
using UnitTestWorkshop.Data.Models.QueryModels;

namespace UnitTestWorkshop.Business.IntegrationTests.Providers
{
    [TestClass]
    public class UserAccountProvider_tests
    {
        private static Mock<ICreatable<User>> _mockCreateableUser;
        private static Mock<IUpdatable<User>> _mockUpdateableUser;
        private static Mock<IRetrievable<ByUserId, User>> _mockRetrievableUserById;
        private static Mock<IDeletable<User>> _mockDeletableUser;
        private static Mock<ISystemTime> _mockSystemTime;
        private static Mock<IBulkRetrievable<ByEncodedUserId, UserAuthentication>> _mockRetrievableAuthyId;
        private static Mock<IUpdatable<UserAuthentication>> _mockUpdatableAuth;
        private static Mock<ICreatable<UserAuthentication>> _mockCreatableAuth;
        private static Mock<IDeletable<UserAuthentication>> _mockDeletableAuth;
        private static Mock<IRetrievable<ByUserEmail, User>> _mockRetrievableUserByEmail;
        private IPasswords _passwords;
        private ITranslate<User, UserAccount> _translateUserUserAuth;
        private IUserAuthenticationProvider _userAuthProvider;
        private UserAccountProvider _subject;

        [ClassInitialize]
        [Description("Initializes only once per full test run.")]
        public static void InitialSetup(TestContext context)
        {
            //needed for account provider
            _mockCreateableUser = new Mock<ICreatable<User>>();
            _mockUpdateableUser = new Mock<IUpdatable<User>>();
            _mockRetrievableUserById = new Mock<IRetrievable<ByUserId, User>>();
            _mockDeletableUser = new Mock<IDeletable<User>>();
            _mockSystemTime = new Mock<ISystemTime>();

            //needed for auth provider
            _mockRetrievableAuthyId = new Mock<IBulkRetrievable<ByEncodedUserId, UserAuthentication>>();
            _mockUpdatableAuth = new Mock<IUpdatable<UserAuthentication>>();
            _mockCreatableAuth = new Mock<ICreatable<UserAuthentication>>();
            _mockDeletableAuth = new Mock<IDeletable<UserAuthentication>>();

            //needed for both account provider and auth provider
            _mockRetrievableUserByEmail = new Mock<IRetrievable<ByUserEmail, User>>();
        }

        [TestInitialize]
        [Description("Initializes before every Test.")]
        public void InitializeProvider()
        {
            _passwords = new Passwords();
            _translateUserUserAuth = new DataUserToAccountUserTranslator();

            _userAuthProvider = new UserAuthenticationProvider(
                _translateUserUserAuth
                , _mockRetrievableUserByEmail.Object
                , _mockRetrievableAuthyId.Object
                , _mockUpdatableAuth.Object
                , _mockCreatableAuth.Object
                , _passwords
                , _mockDeletableAuth.Object);

            _subject = new UserAccountProvider(
                _userAuthProvider
                , _mockCreateableUser.Object
                , _mockRetrievableUserById.Object
                , _mockRetrievableUserByEmail.Object
                , _mockDeletableUser.Object
                , _mockUpdateableUser.Object
                , _translateUserUserAuth
                , _mockSystemTime.Object);
        }

        [TestMethod]
        public void Retrieve_Account_With_Valid_Credentials()
        {
            //Data Layer Arrange
            var dataUser = new User
            {
                CreationDate = new DateTime(2000,01,20),
                Email = "HandyManJack@AOL.com",
                FirstName = "Jack",
                LastName = "Hoffman",
                UserId = "551581bf90eb291bd0e97fb2",
                LastLogin = new DateTime(2012,10,31)
            };

            _mockRetrievableUserById
                .Setup(x => x.Retrieve(It.IsAny<ByUserId>()))
                .Returns(dataUser);

            _mockRetrievableUserByEmail
                .Setup(x => x.Retrieve(It.IsAny<ByUserEmail>()))
                .Returns(dataUser);

            _mockRetrievableAuthyId
                .Setup(x => x.RetrieveAll(It.IsAny<ByEncodedUserId>()))
                .Returns(new List<UserAuthentication>
                {
                    new UserAuthentication
                    {
                        AccountActive = true,
                        AccountType = AccountType.ActualAccount,
                        EncodedPassword = "N6Yblj8+jm04p7AoXji9jxKSq21tsalRS/UBTa8fiV4=",
                        EncodedUserId = "Yvf08ew7hUptyomgslYIYhQmJ+jhUM/nrgQtbJgnW50=",
                        EncryptionKey = "ApplePear",
                        FailedLoginAttemptCount = 0
                    },
                    new UserAuthentication
                    {
                        AccountActive = true,
                        AccountType = AccountType.Trap1Account,
                        EncodedPassword = "Blue",
                        EncodedUserId = "Yvf08ew7hUptyomgslYIYhQmJ+jhUM/nrgQtbJgnW50=",
                        EncryptionKey = "PearBanana",
                        FailedLoginAttemptCount = 0
                    },
                    new UserAuthentication
                    {
                        AccountActive = true,
                        AccountType = AccountType.Trap2Account,
                        EncodedPassword = "Greed",
                        EncodedUserId = "Yvf08ew7hUptyomgslYIYhQmJ+jhUM/nrgQtbJgnW50=",
                        EncryptionKey = "BananaGrape",
                        FailedLoginAttemptCount = 0
                    },
                    new UserAuthentication
                    {
                        AccountActive = true,
                        AccountType = AccountType.Trap3Account,
                        EncodedPassword = "Yellow",
                        EncodedUserId = "Yvf08ew7hUptyomgslYIYhQmJ+jhUM/nrgQtbJgnW50=",
                        EncryptionKey = "GrapeApple",
                        FailedLoginAttemptCount = 0
                    }
                });

            //arrange
            _mockSystemTime
                .Setup(x => x.Current())
                .Returns(new DateTime(2015, 12, 2, 0, 0, 0, DateTimeKind.Utc));

            var input = new Credentials
            {
                Email = "HandyManJack@AOL.com",
                Password = "SnakesAreSlippery"
            };

            var expected = new UserAccount
            {
                Email = "HandyManJack@AOL.com",
                FirstName = "Jack",
                LastName = "Hoffman",
                UserId = "551581bf90eb291bd0e97fb2"
            };

            UserAccount actual;

            //act
            actual = _subject.RetrieveAccount(input);

            //assert
            Assert.AreEqual(expected.Email, actual.Email, string.Format("Expected Email of {0}, actual was {1}", expected.Email, actual.Email));
            Assert.AreEqual(expected.FirstName, actual.FirstName, string.Format("Expected FirstName of {0}, actual was {1}", expected.FirstName, actual.FirstName));
            Assert.AreEqual(expected.LastName, actual.LastName, string.Format("Expected LastName of {0}, actual was {1}", expected.LastName, actual.LastName));
            Assert.AreEqual(expected.UserId, actual.UserId, string.Format("Expected UserId of {0}, actual was {1}", expected.UserId, actual.UserId));
        }
    }
}
