using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stalker.Prey;

namespace AddressBook {
    [NotifyPropertyChanged]
    public class Person {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IList<Address> Addresses { get; set; }
    }
}
