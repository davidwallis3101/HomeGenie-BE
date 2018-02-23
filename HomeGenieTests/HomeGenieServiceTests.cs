using System;
using System.Collections.Generic;
using HomeGenie.Automation.Scheduler;
using HomeGenie.Service;
using NUnit.Framework;

namespace HomeGenieTests
{
    [TestFixture]
    public class HomeGenieServiceTests
    {
        private HomeGenieService _homegenie = null;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _homegenie = new HomeGenieService();
        }


        [Test]
        public void LoadPrograms()
        {
            
        }

        //[Test]
        //public void BasicCronExpression()
        //{
        //    var expression = "0 * * * *";
        //    var occurences = GetOccurencesForDate(_scheduler, _start, expression);

        //    DisplayOccurences(expression, occurences);
        //    Assert.That(occurences.Count, Is.EqualTo(24));
        //}
    }
}