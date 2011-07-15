using Cone;

namespace Concoct.Web
{
    [Describe(typeof(BasicHttpFileCollection))]
    public class BasicHttpFileCollectionSpec
    {
        public void supports_indexing_by_name() {
            var files  = new BasicHttpFileCollection();
            var expected = new BasicHttpPostedFile("expected-file", "text/plain", new byte[0]);
            files.Add(new BasicHttpPostedFile("file-1", "text/plain", new byte[0]));
            files.Add(expected);
            files.Add(new BasicHttpPostedFile("file-3", "text/plain", new byte[0]));

            Verify.That(() => files[expected.FileName] == expected);
        }
    }
}
