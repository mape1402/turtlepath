namespace TurtlePath.Automations.Tests
{
    public class AutomationAssemblyMarkerTests
    {
        [Fact]
        public void Marker_exposes_automations_assembly()
        {
            var assembly = typeof(AutomationAssemblyMarker).Assembly;

            Assert.Equal("TurtlePath.Automations", assembly.GetName().Name);
        }
    }
}
