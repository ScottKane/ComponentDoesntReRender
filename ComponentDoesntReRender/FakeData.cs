using Bogus;
using System;
using System.Collections.Generic;
using static Bogus.DataSets.Name;

namespace ComponentDoesntReRender
{
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class FakeData
    {
        public FakeData() => People = GetPeople();
        public List<Person> People { get; }

        private List<Person> GetPeople()
        {
            Randomizer.Seed = new Random(0);

            return new Faker<Person>()
                .RuleFor(person => person.FirstName, faker => faker.Name.FirstName())
                .RuleFor(person => person.LastName, faker => faker.Name.LastName())
                .Generate(10);
        }
    }
}
