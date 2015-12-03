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
    public class UserAccountProviderSubject
    {
        public static Mock<ICreatable<User>> MockCreateableUser;
        public static Mock<IUpdatable<User>> MockUpdateableUser;
        public static Mock<IRetrievable<ByUserId, User>> MockRetrievableUserById;
        public static Mock<IDeletable<User>> MockDeletableUser;
        public static Mock<ISystemTime> MockSystemTime;
        public static Mock<IBulkRetrievable<ByEncodedUserId, UserAuthentication>> MockRetrievableAuthyId;
        public static Mock<IUpdatable<UserAuthentication>> MockUpdatableAuth;
        public static Mock<ICreatable<UserAuthentication>> MockCreatableAuth;
        public static Mock<IDeletable<UserAuthentication>> MockDeletableAuth;
        public static Mock<IRetrievable<ByUserEmail, User>> MockRetrievableUserByEmail;

        protected static void SetupMocks(TestContext context)
        {
            //needed for account provider
            MockCreateableUser = new Mock<ICreatable<User>>();
            MockUpdateableUser = new Mock<IUpdatable<User>>();
            MockRetrievableUserById = new Mock<IRetrievable<ByUserId, User>>();
            MockDeletableUser = new Mock<IDeletable<User>>();
            MockSystemTime = new Mock<ISystemTime>();

            //needed for auth provider
            MockRetrievableAuthyId = new Mock<IBulkRetrievable<ByEncodedUserId, UserAuthentication>>();
            MockUpdatableAuth = new Mock<IUpdatable<UserAuthentication>>();
            MockCreatableAuth = new Mock<ICreatable<UserAuthentication>>();
            MockDeletableAuth = new Mock<IDeletable<UserAuthentication>>();

            //needed for both account provider and auth provider
            MockRetrievableUserByEmail = new Mock<IRetrievable<ByUserEmail, User>>();
        }

        private IPasswords _passwords;
        private ITranslate<User, UserAccount> _translateUserUserAuth;
        private IUserAuthenticationProvider _userAuthProvider;
        public UserAccountProvider Subject;

        protected void SetupSubject()
        {
            _passwords = new Passwords();
            _translateUserUserAuth = new DataUserToAccountUserTranslator();

            _userAuthProvider = new UserAuthenticationProvider(
                _translateUserUserAuth
                , MockRetrievableUserByEmail.Object
                , MockRetrievableAuthyId.Object
                , MockUpdatableAuth.Object
                , MockCreatableAuth.Object
                , _passwords
                , MockDeletableAuth.Object);

            Subject = new UserAccountProvider(
                _userAuthProvider
                , MockCreateableUser.Object
                , MockRetrievableUserById.Object
                , MockRetrievableUserByEmail.Object
                , MockDeletableUser.Object
                , MockUpdateableUser.Object
                , _translateUserUserAuth
                , MockSystemTime.Object);
        }
    }

    [TestClass]
    public class When_Retrieving_An_Account_With_Valid_Credentials : UserAccountProviderSubject
    {
        [ClassInitialize]
        [Description("Initializes only once per full test run.")]
        public static void InitialSetup(TestContext context)
        {
            SetupMocks(context);
        }

        [TestInitialize]
        [Description("Initializes before every Test.")]
        public void InitializeProvider()
        {
            SetupSubject();
        }

        [TestMethod]
        public void Should_Return_Account()
        {
            //Data Layer Arrange
            var dataUser = new User
            {
                CreationDate = new DateTime(2000, 01, 20),
                Email = "HandyManJack@AOL.com",
                FirstName = "Jack",
                LastName = "Hoffman",
                UserId = "551581bf90eb291bd0e97fb2",
                LastLogin = new DateTime(2012, 10, 31)
            };

            MockRetrievableUserById
                .Setup(x => x.Retrieve(It.IsAny<ByUserId>()))
                .Returns(dataUser);

            MockRetrievableUserByEmail
                .Setup(x => x.Retrieve(It.IsAny<ByUserEmail>()))
                .Returns(dataUser);

            MockRetrievableAuthyId
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
            MockSystemTime
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
            actual = Subject.RetrieveAccount(input);

            //assert
            Assert.AreEqual(expected.Email, actual.Email, string.Format("Expected Email of {0}, actual was {1}", expected.Email, actual.Email));
            Assert.AreEqual(expected.FirstName, actual.FirstName, string.Format("Expected FirstName of {0}, actual was {1}", expected.FirstName, actual.FirstName));
            Assert.AreEqual(expected.LastName, actual.LastName, string.Format("Expected LastName of {0}, actual was {1}", expected.LastName, actual.LastName));
            Assert.AreEqual(expected.UserId, actual.UserId, string.Format("Expected UserId of {0}, actual was {1}", expected.UserId, actual.UserId));
        }
    }

    [TestClass]
    public class When_Retrieving_An_Account_With_InValid_Credentials : UserAccountProviderSubject
    {
        [ClassInitialize]
        [Description("Initializes only once per full test run.")]
        public static void InitialSetup(TestContext context)
        {
            SetupMocks(context);
        }

        [TestInitialize]
        [Description("Initializes before every Test.")]
        public void InitializeProvider()
        {
            SetupSubject();
        }

        [TestMethod]
        public void Should_Return_Null()
        {
            var dataUser = new User
            {
                CreationDate = new DateTime(2000, 01, 20),
                Email = "HandyManJack@AOL.com",
                FirstName = "Jack",
                LastName = "Hoffman",
                UserId = "551581bf90eb291bd0e97fb2",
                LastLogin = new DateTime(2012, 10, 31)
            };

            MockRetrievableUserById
                .Setup(x => x.Retrieve(It.IsAny<ByUserId>()))
                .Returns(dataUser);

            MockRetrievableUserByEmail
                .Setup(x => x.Retrieve(It.IsAny<ByUserEmail>()))
                .Returns(dataUser);

            MockRetrievableAuthyId
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
            MockSystemTime
                .Setup(x => x.Current())
                .Returns(new DateTime(2015, 12, 2, 0, 0, 0, DateTimeKind.Utc));

            var input = new Credentials
            {
                Email = "HandyManJack@AOL.com",
                Password = "SnakesAreSlipper"
            };

            UserAccount actual;

            //act
            actual = Subject.RetrieveAccount(input);

            //assert
            Assert.IsNull(actual, "Login should have failed and returned a null object.");
        }

        [TestMethod]
        public void Should_Not_Update_Last_Login()
        {
            var dataUser = new User
            {
                CreationDate = new DateTime(2000, 01, 20),
                Email = "HandyManJack@AOL.com",
                FirstName = "Jack",
                LastName = "Hoffman",
                UserId = "551581bf90eb291bd0e97fb2",
                LastLogin = new DateTime(2012, 10, 31)
            };

            MockRetrievableUserById
                .Setup(x => x.Retrieve(It.IsAny<ByUserId>()))
                .Returns(dataUser);

            MockRetrievableUserByEmail
                .Setup(x => x.Retrieve(It.IsAny<ByUserEmail>()))
                .Returns(dataUser);

            MockRetrievableAuthyId
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
            MockSystemTime
                .Setup(x => x.Current())
                .Returns(new DateTime(2015, 12, 2, 0, 0, 0, DateTimeKind.Utc));

            var input = new Credentials
            {
                Email = "HandyManJack@AOL.com",
                Password = "SnakesAreSlipper"
            };

            UserAccount actual;

            //act
            actual = Subject.RetrieveAccount(input);

            //assert
            MockRetrievableUserById.Verify(x => x.Retrieve(It.IsAny<ByUserId>()),Times.Never());
            MockUpdateableUser.Verify(x => x.Update(It.IsAny<User>()),Times.Never);
            MockUpdateableUser.Verify(x => x.Save(), Times.Never);
        }

        [TestMethod]
        public void Should_Process_Failed_Attempt()
        {
            var dataUser = new User
            {
                CreationDate = new DateTime(2000, 01, 20),
                Email = "HandyManJack@AOL.com",
                FirstName = "Jack",
                LastName = "Hoffman",
                UserId = "551581bf90eb291bd0e97fb2",
                LastLogin = new DateTime(2012, 10, 31)
            };

            MockRetrievableUserById
                .Setup(x => x.Retrieve(It.IsAny<ByUserId>()))
                .Returns(dataUser);

            MockRetrievableUserByEmail
                .Setup(x => x.Retrieve(It.IsAny<ByUserEmail>()))
                .Returns(dataUser);

            MockRetrievableAuthyId
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
            MockSystemTime
                .Setup(x => x.Current())
                .Returns(new DateTime(2015, 12, 2, 0, 0, 0, DateTimeKind.Utc));

            var input = new Credentials
            {
                Email = "HandyManJack@AOL.com",
                Password = "SnakesAreSlipper"
            };

            UserAccount actual;

            //act
            actual = Subject.RetrieveAccount(input);

            //assert
            MockUpdatableAuth.Verify(x=>x.Update(It.Is<UserAuthentication>(i=> i.FailedLoginAttemptCount == 1)), Times.Exactly(4));
            MockUpdatableAuth.Verify(x=>x.Save(),Times.Once);
        }
    }
}
