using Cone;

namespace Concoct.Web
{
    [Describe(typeof(BasicHttpFileCollection))]
    public class BasicHttpFileCollectionSpec
    {
        public void supports_indexing_by_key() {
            var files  = new BasicHttpFileCollection();
            var expected = new BasicHttpPostedFile("expected-file", "text/plain", new byte[0]);
            files.Add("file-1", new BasicHttpPostedFile("file-1", "text/plain", new byte[0]));
            files.Add("expected", expected);
            files.Add("file-3", new BasicHttpPostedFile("file-3", "text/plain", new byte[0]));

            Verify.That(() => files["expected"] == expected);
        }
    }
}
