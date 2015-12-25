// ReSharper disable All
using System;
using System.Diagnostics;
using System.Linq;
using Frapid.ApplicationState.Models;
using Frapid.Account.Api.Fakes;
using Xunit;

namespace Frapid.Account.Api.Tests
{
    public class CanConfirmRegistrationTests
    {
        public static CanConfirmRegistrationController Fixture()
        {
            CanConfirmRegistrationController controller = new CanConfirmRegistrationController(new CanConfirmRegistrationRepository());
            return controller;
        }

        [Fact]
        [Conditional("Debug")]
        public void Execute()
        {
            var actual = Fixture().Execute(new CanConfirmRegistrationController.Annotation());
            Assert.Equal(new bool(), actual);
        }
    }
}