using System;
using System.Xml.Schema;

using NUnit.Framework;

using SharpBrake.Serialization;

namespace SharpBrake.Tests
{
    [TestFixture]
    public class SchemaValidation
    {
        [Test]
        public void Maximal_notice_generates_valid_XML()
        {
            var error = Activator.CreateInstance<AirbrakeError>();
            error.Class = "TestError";
            error.Message = "something blew up";
            error.Backtrace = new[]
            {
                new AirbrakeTraceLine("unknown.cs", 0) { Method = "unknown" }
            };

            var notice = new AirbrakeNotice
            {
                ApiKey = "123456",
                Error = error,
                Request = new AirbrakeRequest(new Uri("http://example.com/"), GetType().FullName)
                {
                    Action = "Maximal_notice_generates_valid_XML",
                    Component = "MyApp.HomeController",
                    CgiData = new[]
                    {
                        new AirbrakeVar("REQUEST_METHOD", "POST"),
                    },
                    Params = new[]
                    {
                        new AirbrakeVar("Form.Key1", "Form.Value1"),
                    },
                    Session = new[]
                    {
                        new AirbrakeVar("UserId", "1"),
                    },
                    Url = "http://example.com/myapp",
                },
                Notifier = new AirbrakeNotifier
                {
                    Name = "hopsharp",
                    Version = "2.0",
                    Url = "http://github.com/krobertson/hopsharp",
                },
                ServerEnvironment = new AirbrakeServerEnvironment("staging")
                {
                    ProjectRoot = "/test",
                },
            };

            var serializer = new CleanXmlSerializer<AirbrakeNotice>();
            string xml = serializer.ToXml(notice);

            AirbrakeValidator.ValidateSchema(xml);
        }


        [Test]
        public void Minimal_notice_generates_valid_XML()
        {
            var error = Activator.CreateInstance<AirbrakeError>();
            error.Class = "TestError";
            error.Message = "something blew up";
            error.Backtrace = new[]
            {
                new AirbrakeTraceLine("unknown.cs", 0) { Method = "unknown" }
            };

            var notice = new AirbrakeNotice
            {
                ApiKey = "123456",
                Error = error,
                Notifier = new AirbrakeNotifier
                {
                    Name = "hopsharp",
                    Version = "2.0",
                    Url = "http://github.com/krobertson/hopsharp"
                },
                ServerEnvironment = new AirbrakeServerEnvironment("staging")
                {
                    ProjectRoot = "/test",
                },
            };

            var serializer = new CleanXmlSerializer<AirbrakeNotice>();
            string xml = serializer.ToXml(notice);

            AirbrakeValidator.ValidateSchema(xml);
        }


        [Test]
        public void Notice_missing_error_fails_validation()
        {
            var notice = new AirbrakeNotice
            {
                ApiKey = "123456",
                Request = new AirbrakeRequest(new Uri("http://example.com/"), GetType().FullName)
                {
                    Action = "Maximal_notice_generates_valid_XML",
                },
                Notifier = new AirbrakeNotifier
                {
                    Name = "hopsharp",
                    Version = "2.0",
                    Url = "http://github.com/krobertson/hopsharp"
                },
                ServerEnvironment = new AirbrakeServerEnvironment("staging")
                {
                    ProjectRoot = "/test",
                },
            };

            var serializer = new CleanXmlSerializer<AirbrakeNotice>();
            string xml = serializer.ToXml(notice);

            TestDelegate throwing = () => AirbrakeValidator.ValidateSchema(xml);
            XmlSchemaValidationException exception = Assert.Throws<XmlSchemaValidationException>(throwing);

            Assert.That(exception.Message, Is.StringContaining("notice"));
            Assert.That(exception.Message, Is.StringContaining("error"));
            Assert.That(exception.LineNumber, Is.EqualTo(17));
            Assert.That(exception.LinePosition, Is.EqualTo(3));
        }
    }
}