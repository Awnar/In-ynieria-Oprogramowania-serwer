using IO_2_lib;
using IO_2_lib.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;

namespace IO_2_Lib.Tests
{
    [TestClass]
    public class SQLiteTests
    {
        private DatabaseContext _database;
        private User _testUser;
        private const string _testUserName = "testUser";
        private const string _testUserPassword = "testPass";

        [TestInitialize]
        public void TestInitialize()
        {
            DatabaseContext.IsInMemory = true;
            _database = new DatabaseContext();
            _database.Database.EnsureCreated();
            SQLite.Init(_database);

            PopulateWithTestData();
            _testUser = GetTestUser();
        }

        [TestMethod]
        public void Register_InputIsCorrect_CreateUser()
        {
            var resultFromMethod = SQLite.Register("user", "testpass");
            var isInDatabase = _database.Users.Any(x => x.Name == "user" && x.Password == CreateMD5("testpass"));

            Assert.IsTrue(resultFromMethod);
            Assert.IsTrue(isInDatabase);
        }

        [TestMethod]
        public void Login_InputIsCorrect_ReturnUserId()
        {
            var resultFromMethod = SQLite.Login(_testUser.Name, _testUserPassword);
            var userFromDb = _database.Users.FirstOrDefault(x => x.Name == _testUser.Name && x.Password == _testUser.Password);

            Assert.AreEqual(resultFromMethod, userFromDb.Id);
        }

        [TestMethod]
        public void Login_PasswordIsincorrect_ReturnNegativeNumber()
        {
            var resultFromMethod = SQLite.Login(GetTestUser().Name, GetTestUser().Password);
            Assert.IsTrue(resultFromMethod < 0);
        }

        [DataTestMethod]
        [DataRow(_testUserName, true)]
        [DataRow("NonExisting", false)]
        public void CheckName_UsernameGiven_ReturnBool(string userName, bool exists)
        {
            var resultFromMethod = SQLite.checkname(userName);
            Assert.AreEqual(resultFromMethod, exists);
        }

        [TestMethod]
        public void ListTask_ExistingUser_ReturnTaskList()
        {
            var userTasks = SQLite.ListTasks(_testUser.Id);
            var userTasksFromDb = _database.Users.Find(_testUser.Id).Tasks;
            Assert.IsTrue(userTasks.Count() > 0);
            CollectionAssert.AreEqual(userTasks, userTasksFromDb.ToList());
        }

        [TestMethod]
        public void GetTask_ExistingUser_ReturnTaskDetails()
        {
            var userTask = SQLite.GetTask(1, _testUser.Id);
            var userTaskFromDb = _database.Users.Find(_testUser.Id).Tasks.First(x => x.Id == 1);
            Assert.AreEqual(userTask, userTaskFromDb);
        }

        [TestMethod]
        public void DelTask_ExistingTask_CorrectlyDeletesTask()
        {
            var taskCountedBefore = _database.Users.Find(_testUser.Id).Tasks.Count();
            SQLite.DelTask(1, _testUser.Id);
            var taskCountedAfter = _database.Users.Find(_testUser.Id).Tasks.Count();
            var findDeletedTask = _database.Tasks.Find(1);

            Assert.IsTrue(taskCountedBefore == taskCountedAfter + 1);
            Assert.IsNull(findDeletedTask);
        }

        [TestMethod]
        public void UpdateTask_ExistingTask_CorrectlyUpdatesTask()
        {
            var taskNameBeforeUpdate = _database.Tasks.Find(1).Name;
            var taskDescriptionBeforeUpdate = _database.Tasks.Find(1).Description;

            SQLite.UpdateTask(_testUser.Id,1, "NewName", "NewDesc");
            var taskAfterUpdate = _database.Tasks.Find(1);

            Assert.AreNotEqual(taskNameBeforeUpdate, taskAfterUpdate.Name);
            Assert.AreNotEqual(taskDescriptionBeforeUpdate, taskAfterUpdate.Description);

            Assert.AreEqual(taskAfterUpdate.Name, "NewName");
            Assert.AreEqual(taskAfterUpdate.Description, "NewDesc");
        }

        private void PopulateWithTestData()
        {
            var user = new User()
            {
                Id = 1,
                Name = _testUserName,
                Password = CreateMD5(_testUserPassword)
            };

            user.AuthKey = new AuthKey()
            {
                Key = "1234",
                LastAccessIP = "0.0.0.0",
                Expiration = DateTime.Now.Add(new TimeSpan(0,10,0))
            };

            user.Tasks.Add(new Task()
            {
                Id = 1,
                Name = "Task1",
                Description = "Description1",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now
            });
            user.Tasks.Add(new Task()
            {
                Id = 2,
                Name = "Task2",
                Description = "Description2",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now
            });

            _database.Users.Add(user);

            _database.SaveChanges();
        }

        private User GetTestUser()
        {
            return _database.Users.Find(1);
        }

        private static string CreateMD5(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

    }
}
