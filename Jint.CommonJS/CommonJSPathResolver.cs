using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Jint.CommonJS
{
    public class CommonJSPathResolver : IModuleResolver
    {
        private readonly IEnumerable<string> extensionHandlers;

        public CommonJSPathResolver(IEnumerable<string> extensionHandlers)
        {
            this.extensionHandlers = extensionHandlers;
        }

        public string ResolvePath(string moduleId, Module parent)
        {
            bool isExternalModule = !moduleId.StartsWith(".") && parent.filePath != null;

            var cwd = parent.filePath != null ? Path.GetDirectoryName(parent.filePath) : Environment.CurrentDirectory;

            if (isExternalModule)
            {
                // for external modules we look in the directory of the main module
                var rootModule = parent.ParentModule;

                while (rootModule.ParentModule != null)
                {
                    rootModule = rootModule.ParentModule;
                }

                cwd = new DirectoryInfo(rootModule.Id).Parent.FullName;
            }

            var path = Path.Combine(cwd, moduleId);

            /*
             * - Try direct file in case an extension is provided
             * - if directory, return directory/index
             */

            if (Directory.Exists(path))
            {
                path = Path.Combine(path, "index");
            }

            if (!File.Exists(path))
            {
                foreach (var tryExtension in extensionHandlers.Where(i => i != "default"))
                {
                    string innerCandidate = path + tryExtension;
                    if (File.Exists(innerCandidate))
                    {
                        return innerCandidate;
                    }
                }

                throw new FileNotFoundException($"Module {path} could not be resolved.");
            }

            return path;
        }
    }
}