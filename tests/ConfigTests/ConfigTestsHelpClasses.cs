namespace TestsArgparseAPI.ConfigTests
{
    public class Person
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }

    public record TestConfiguration
    {
        // properties for flags
        public bool Help = false;
        public bool Version = false;

        // properties for options
        public int IntOption = 0;
        public string StringOption = "";
        public Person PersonOption = new();

        // properties fro arguments
        public int IntArgument = 0;
        public string StringArgument = "";
        public Person PersonArgument = new Person();

        /// <summary>
        /// Expects input in format: name,address.
        /// </summary>
        /// <param name="value">String value to parse.</param>
        /// <returns></returns>
        public static Person PersonConvertor(string value)
        {
            var parsed = value.Split(',');
            return new Person { Name = parsed[0], Address = parsed[1] };
        }
    }
}
