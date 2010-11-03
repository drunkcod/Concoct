using System.IO;
using Cone;
using System.Collections.Generic;

namespace Concoct.Web
{
    [Describe(typeof(ConcoctHttpServerUtility))]
    public class ConcoctHttpServerUtilitySpec
    {
        const string PhysicalPath = "X:\\Some\\Path";
        ConcoctHttpServerUtility Server = new ConcoctHttpServerUtility(PhysicalPath);

        public void MapPath(string path, string expected) {
            Verify.That(() => Server.MapPath(path) == expected);
        }

        public IEnumerable<IRowTestData> MapPathRows(){
            return new RowBuilder<ConcoctHttpServerUtilitySpec>()
            .Add(x => x.MapPath("", PhysicalPath))
            .Add(x => x.MapPath(".", PhysicalPath))
            .Add(x => x.MapPath("~", PhysicalPath))
            .Add(x => x.MapPath("Foo", Path.Combine(PhysicalPath, "Foo")))
            .Add(x => x.MapPath("~/Foo", Path.Combine(PhysicalPath, "Foo")));
        }
    }
}
