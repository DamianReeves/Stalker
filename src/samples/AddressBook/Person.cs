using System.Collections.Generic;
using ReactiveUI;

namespace AddressBook {
    public class Person: ReactiveObject {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IList<Address> Addresses { get; set; }
    }
}
