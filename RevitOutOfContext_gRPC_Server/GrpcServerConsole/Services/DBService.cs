namespace GrpcServerConsole.Services
{
    public class DBService
    {
        public static void RecordFamiliesDB(List<(string, string)> famNamesAndCats)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                foreach(var famAndCat  in famNamesAndCats)
                {
                    if (!db.families.Select(family => family.Name).Contains(famAndCat.Item1))
                    {
                        var family = new Family(famAndCat.Item1, famAndCat.Item2);
                        db.families.Add(family);
                    }
                }
                db.SaveChanges();
            }
        }
    }
}
