using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cone;

namespace Concoct
{
    [Describe(typeof(Program))]
    public class ProgramSpec
    {
        [Context("command line parsing")]
        public class CommandLineParsing
        {
            string AssemblyName { get { return typeof(Program).Assembly.Location; } }
            const string VirtualDirectory = "/VirtualDirectory";

            ConcoctConfiguration ParseConfiguration(params string[] args) {
                return Program.ParseConfiguration(args);
            }

            public void first_argument_is_assembly_name() {
                Verify.That(() => ParseConfiguration(AssemblyName, VirtualDirectory).ApplicationAssemblyPath == AssemblyName);
            }

            public void second_argument_is_prefix_or_vdir() {
                Verify.That(() => ParseConfiguration(AssemblyName, VirtualDirectory).VirtualDirectoryOrPrefix == VirtualDirectory);
            }

            public void port_parameter() {
                Verify.That(() => ParseConfiguration(AssemblyName, VirtualDirectory, "--port=8181").Port == 8181);
            }
        }
    }
}
