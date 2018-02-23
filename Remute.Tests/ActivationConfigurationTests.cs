using Microsoft.VisualStudio.TestTools.UnitTesting;
using Remute.Tests.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Remute.Tests
{
    [TestClass]
    public class ActivationConfigurationTests
    {
        [TestMethod]
        public void ConfigureByConstructor_Success()
        {
            var user = new User(1, "Joe", "Doe");
            var type = user.GetType();
            var constructor = type.GetConstructor(new[] { typeof(int), typeof(string), typeof(string) });

            var config = new ActivationConfiguration()
                .Configure(constructor);

            var remute = new Remute(config);

            var actual = remute.With(user, x => x.FirstName, "Foo");

            Assert.AreEqual("Foo", actual.FirstName);
            Assert.AreEqual(user.LastName, actual.LastName);
            Assert.AreEqual(user.Id, actual.Id);
        }

        [TestMethod]
        public void ConfigureByConstructorAndParameters_Success()
        {
            var user = new User(1, "Joe", "Doe");
            var type = user.GetType();
            var constructor = type.GetConstructor(new[] { typeof(string), typeof(string) });

            var firstNameParameter = constructor.GetParameters().Single(x => x.Name == "firstNameNotMatchingPropertyName");
            var firstNameProperty = type.GetProperty("FirstName");

            var config = new ActivationConfiguration()
                .Configure(constructor, new Dictionary<ParameterInfo, PropertyInfo>() { [firstNameParameter] = firstNameProperty });

            var remute = new Remute(config);

            var actual = remute.With(user, x => x.FirstName, "Foo");

            Assert.AreEqual("Foo", actual.FirstName);
            Assert.AreEqual(user.LastName, actual.LastName);
            Assert.AreEqual(default(int), actual.Id);
        }

        [TestMethod]
        public void ConfigureByExpression_Success()
        {
            var user = new User(1, "Joe", "Doe");

            var config = new ActivationConfiguration()
                .Configure<User>(x => new User(x.FirstName, x.LastName));

            var remute = new Remute(config);

            var actual = remute.With(user, x => x.FirstName, "Foo");

            Assert.AreEqual("Foo", actual.FirstName);
            Assert.AreEqual(user.LastName, actual.LastName);
            Assert.AreEqual(default(int), actual.Id);
        }

        [TestMethod]
        public void ConfigureByConstructorAndParameters_InvalidParameter_Fail()
        {
            var user = new User(1, "Joe", "Doe");
            var constructor = user.GetType().GetConstructor(new[] { typeof(string), typeof(string) });

            var type = typeof(Employee);
            var firstNameParameter = type.GetConstructor(new[] { typeof(Guid), typeof(string), typeof(string) }).GetParameters().Single(x => x.Name == "firstName");
            var firstNameProperty = type.GetProperty("FirstName");

            try
            {
                var config = new ActivationConfiguration()
                    .Configure(constructor, new Dictionary<ParameterInfo, PropertyInfo>() { [firstNameParameter] = firstNameProperty });
            }
            catch (Exception ex) when (ex.Message == $"Invalid parameter '{firstNameParameter.Name}'. Parameter must be a member of '{constructor.DeclaringType}' constructor.")
            {
                return;
            }

            Assert.Fail();
        }

        [TestMethod]
        public void ConfigureByConstructorAndParameters_InvalidProperty_Fail()
        {
            var user = new User(1, "Joe", "Doe");
            var constructor = user.GetType().GetConstructor(new[] { typeof(int), typeof(string), typeof(string) });

            var firstNameParameter = constructor.GetParameters().Single(x => x.Name == "firstName");
            var firstNameProperty = typeof(Employee).GetProperty("FirstName");

            try
            {
                var config = new ActivationConfiguration()
                    .Configure(constructor, new Dictionary<ParameterInfo, PropertyInfo>() { [firstNameParameter] = firstNameProperty });
            }
            catch (Exception ex) when (ex.Message == $"Invalid property '{firstNameProperty.Name}'. Must be a member of '{constructor.DeclaringType}'.")
            {
                return;
            }

            Assert.Fail();
        }

        [TestMethod]
        public void ConfigureByExpression_InvalidExpression_Fail()
        {
            var user = new User(1, "Joe", "Doe");

            try
            {
                var config = new ActivationConfiguration()
                    .Configure<User>(x => x);
            }
            catch (Exception ex) when (ex.Message == $"Expression must specify constructor of '{typeof(User)}'.")
            {
                return;
            }

            Assert.Fail();
        }

        [TestMethod]
        public void ConfigureByExpression_InvalidProperty_Fail()
        {
            var user = new User(1, "Joe", "Doe");

            try
            {
                var config = new ActivationConfiguration()
                    .Configure<User>(x => new User("Foo", "Bar"));
            }
            catch (Exception ex) when (ex.Message == $"Parameter \"Foo\" must be a property of '{typeof(User)}'.")
            {
                return;
            }

            Assert.Fail();
        }
    }
}
