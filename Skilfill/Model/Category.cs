using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilfill.Model
{
   public class Category
    {
        public string[] data { get; set; }
        
    }
    public class Magazine
    {
        public List<MagazineData> data { get; set; }
    }
    public class MagazineData
    {
       
        public int id { get; set; }
        public string name { get; set; }
        public string category { get; set; } 

    }

    public class subscribers
    {
        public List<SubscribersData> data { get; set; }
    }
    public class SubscribersData
    {

        public string id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public int[] magazineIds { get; set; }

      //  public List<magazineList> magazineIds { get; set; }




        //   "id":"527ab86d-f1b7-414f-a9af-93e819667588","firstName":"Hannah","lastName":"Smith","magazineIds":[9,3,1,4,5]
    }
    public class magazineList
    {
        public string magazineId { get; set; }
    }


}
