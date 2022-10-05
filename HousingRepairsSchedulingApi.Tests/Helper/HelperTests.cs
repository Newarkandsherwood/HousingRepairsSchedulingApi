namespace HousingRepairsSchedulingApi.Tests.HelperTests
{
    using System;
    using Helpers;
    using Xunit;

    public class HelperTests
    {

        [Fact]
        public  void GivenTextIsOverCharacterLimit()
        {
            const string textOver255Characters = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis,.";
            Assert.Throws<Exception>(() => Helper.VerifyLengthIsUnder255Characters(textOver255Characters));
        }

        [Fact]
        public  void GivenTextIsUnderCharacterLimit()
        {
            const string textOver255Characters = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit.";
            var exception = Record.Exception(() =>Helper.VerifyLengthIsUnder255Characters(textOver255Characters));
            Assert.Null(exception);
        }
    }
}
