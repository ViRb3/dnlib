/*
    Copyright (C) 2012-2014 de4dot@gmail.com

    Permission is hereby granted, free of charge, to any person obtaining
    a copy of this software and associated documentation files (the
    "Software"), to deal in the Software without restriction, including
    without limitation the rights to use, copy, modify, merge, publish,
    distribute, sublicense, and/or sell copies of the Software, and to
    permit persons to whom the Software is furnished to do so, subject to
    the following conditions:

    The above copyright notice and this permission notice shall be
    included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
    CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
    TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
    SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using dnlib.DotNet;

namespace dnlib.Utils
{
    /// <summary>
    ///     Extension methods
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        ///     Regex to remove the parameter counter at the end of types' names if they have parameters
        /// </summary>
        private static readonly Regex ParameterCount = new Regex(@"(`[0-9]{1,}$)|(`[0-9]{1,}\[\]$)");

        /// <summary>
        ///     Returns the extended name of the type signature so to show full arguments
        /// </summary>
        /// <param name="typeSig">Type signature to process</param>
        /// <returns>The extended name</returns>
        public static string GetExtendedName(this TypeSig typeSig)
        {
            if (typeSig == null) return "";

            string name = typeSig.TypeName;
            name = ParameterCount.Replace(name, string.Empty);

            if (typeSig.ToGenericInstSig() != null)
            {
                name += "<";

                IList<TypeSig> genericArguments = typeSig.ToGenericInstSig().GenericArguments;

                for (int i = 0; i < genericArguments.Count; i++)
                {
                    string newArgs = genericArguments[i].GetExtendedName();

                    if (newArgs != string.Empty)
                        name += newArgs;
                    else
                    {
                        name += genericArguments[i].TypeName;
                        name = ParameterCount.Replace(name, string.Empty);
                    }

                    if (i < genericArguments.Count - 1)
                        name += ", ";
                }

                name += ">";
            }

            return name;
        }

        /// <summary>
        ///     Returns the extended name of the type so to show full arguments
        /// </summary>
        /// <param name="type">Type to process</param>
        /// <returns>The extended name</returns>
        public static string GetExtendedName(this TypeDef type)
        {
            string name = type.Name;
            name = ParameterCount.Replace(name, string.Empty);

            if (type.HasGenericParameters)
            {
                name += "<";

                IList<GenericParam> args = type.GenericParameters;

                for (int i = 0; i < args.Count; i++)
                {
                    name += args[i].Name;
                    name = ParameterCount.Replace(name, string.Empty);

                    if (i < args.Count - 1)
                        name += ", ";
                }

                name += ">";
            }

            if (type.BaseType != null)
            {
                name += string.Format(": {0}", type.BaseType.ToTypeSig().GetExtendedName());
            }

            return name;
        }

        /// <summary>
        ///     Returns the extended name of the method so to show full parameters
        /// </summary>
        /// <param name="method">Method to process</param>
        /// <param name="includeReturnType">Whether to include the return type in the extended name</param>
        /// <returns>The extended name</returns>
        public static string GetExtendedName(this MethodDef method, bool includeReturnType)
        {
            var parameters = "";

            foreach (Parameter parameter in method.Parameters)
            {
                if (parameter.IsHiddenThisParameter)
                    continue;
                
                parameters += parameter.Type.GetExtendedName();
                parameters += ", ";
            }

            parameters = parameters.TrimEnd(',', ' ');

            if (includeReturnType)
                return (string.Format("{0}({1}): {2}", method.Name, parameters, method.ReturnType.GetExtendedName()));

            return (string.Format("{0}({1})", method.Name, parameters));
        }

        /// <summary>
        ///     Creates a list with all accessors of a type (Property methods, Event methods ...)
        /// </summary>
        /// <param name="type">Type to process</param>
        /// <returns>List with all accessors</returns>
        public static List<MethodDef> GetAccessorMethods(this TypeDef type)
        {
            var accessorMethods = new List<MethodDef>();
            foreach (PropertyDef property in type.Properties)
            {
                accessorMethods.Add(property.GetMethod);
                accessorMethods.Add(property.SetMethod);
                if (property.HasOtherMethods)
                {
                    foreach (MethodDef m in property.OtherMethods)
                        accessorMethods.Add(m);
                }
            }
            foreach (EventDef ev in type.Events)
            {
                accessorMethods.Add(ev.AddMethod);
                accessorMethods.Add(ev.RemoveMethod);
                accessorMethods.Add(ev.InvokeMethod);
                if (ev.HasOtherMethods)
                {
                    foreach (MethodDef m in ev.OtherMethods)
                        accessorMethods.Add(m);
                }
            }
            return accessorMethods;
        }

        /// <summary>
        /// Changes the AssemblyInformation of an assembly
        /// </summary>
        /// <param name="assembly">Assembly to modify</param>
        /// <param name="assemblyInformation">AssemblyInformation to apply</param>
        /// <param name="patchResourceInfo">Whether to also patch the resource version information</param>
        public static void ChangeAsmInformation(AssemblyDef assembly, AssemblyInformation assemblyInformation, bool patchResourceInfo = false)
        {
            if (assemblyInformation.AssemblyCompany != null)
                ChangeAsmInformationAttribute(assembly, typeof(AssemblyCompanyAttribute),
                assemblyInformation.AssemblyCompany);

            if (assemblyInformation.AssemblyConfiguration != null)
                ChangeAsmInformationAttribute(assembly, typeof(AssemblyConfigurationAttribute),
                assemblyInformation.AssemblyConfiguration);

            if (assemblyInformation.AssemblyCopyright != null)
                ChangeAsmInformationAttribute(assembly, typeof(AssemblyCopyrightAttribute),
                assemblyInformation.AssemblyCopyright);

            if (assemblyInformation.AssemblyDescription != null)
                ChangeAsmInformationAttribute(assembly, typeof(AssemblyDescriptionAttribute),
                assemblyInformation.AssemblyDescription);

            if (assemblyInformation.AssemblyFileVersion != null)
                ChangeAsmInformationAttribute(assembly, typeof(AssemblyFileVersionAttribute),
                assemblyInformation.AssemblyFileVersion);

            if (assemblyInformation.AssemblyProduct != null)
                ChangeAsmInformationAttribute(assembly, typeof(AssemblyProductAttribute),
                assemblyInformation.AssemblyProduct);

            if (assemblyInformation.AssemblyTitle != null)
                ChangeAsmInformationAttribute(assembly, typeof(AssemblyTitleAttribute),
                assemblyInformation.AssemblyTitle);

            if (assemblyInformation.AssemblyTrademark != null)
                ChangeAsmInformationAttribute(assembly, typeof(AssemblyTrademarkAttribute),
                assemblyInformation.AssemblyTrademark);

            if (assemblyInformation.AssemblyVersion != null)
                ChangeAsmInformationAttribute(assembly, typeof(AssemblyVersionAttribute),
                assemblyInformation.AssemblyVersion);
        }

        /// <summary>
        /// Changes the value of the first ConstructorArgument of a CustomAttribute with a name matching the
        /// full name of its base type, or creates a new one if missing
        /// </summary>
        /// <param name="assembly">Assembly to modify</param>
        /// <param name="type">Type of the attribute</param>
        /// <param name="value">New value to apply to the attribute's first ConstructorArgument</param>
        public static void ChangeAsmInformationAttribute(AssemblyDef assembly, Type type, string value)
        {
            if (value == null)
                throw new NullReferenceException();

            var attribute = assembly.CustomAttributes.Find(type.FullName);

            if (attribute == null || attribute.Constructor == null)
            {
                var typeRef = new TypeRefUser(assembly.ManifestModule, "System.Reflection", type.Name, assembly.ManifestModule.CorLibTypes.AssemblyRef);
                var ctorRef = new MemberRefUser(assembly.ManifestModule, ".ctor",
                            MethodSig.CreateStatic(assembly.ManifestModule.CorLibTypes.Void, assembly.ManifestModule.CorLibTypes.String),
                            typeRef);

                var argument = new CAArgument(assembly.ManifestModule.CorLibTypes.String, value);
                attribute = new CustomAttribute(ctorRef, new List<CAArgument> { argument });

                assembly.CustomAttributes.Add(attribute);
            }
            else if (attribute.ConstructorArguments.Count < 1)
            {
                var argument = new CAArgument(assembly.ManifestModule.CorLibTypes.String, value);
                var newAttribute = new CustomAttribute(attribute.Constructor, new List<CAArgument> { argument });

                assembly.CustomAttributes.Remove(attribute);
                assembly.CustomAttributes.Add(newAttribute);

            }
            else
            {
                var argument = attribute.ConstructorArguments[0];

                argument.Value = value;
                attribute.ConstructorArguments[0] = argument;
            }
        }

        /// <summary>
        /// Changes the resource version information of an assembly
        /// </summary>
        /// <param name="assembly">Assembly to modify</param>
        /// <param name="versionResInformation">AssemblyInformation to apply</param>
        public static void ChangeResourceInformation(ref AssemblyDef assembly, VersionResourceInformation versionResInformation)
        {
            string tempFilePath = DropVersionChanger();

            string tempAssemblyPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".exe");
            assembly.Write(tempAssemblyPath);

            if (versionResInformation.CompanyName != null)
                ChangeResourceInformationAttribute(tempAssemblyPath, tempFilePath, "CompanyName",
                    versionResInformation.CompanyName);

            if (versionResInformation.FileDescription != null)
                ChangeResourceInformationAttribute(tempAssemblyPath, tempFilePath, "FileDescription",
                    versionResInformation.FileDescription);

            if (versionResInformation.FileVersion != null)
                ChangeResourceInformationFileVersion(tempAssemblyPath, tempFilePath, versionResInformation.FileVersion);

            if (versionResInformation.InternalName != null)
                ChangeResourceInformationAttribute(tempAssemblyPath, tempFilePath, "InternalName",
                    versionResInformation.InternalName);

            if (versionResInformation.LegalCopyright != null)
                ChangeResourceInformationAttribute(tempAssemblyPath, tempFilePath, "LegalCopyright",
                    versionResInformation.LegalCopyright);

            if (versionResInformation.ProductName != null)
                ChangeResourceInformationAttribute(tempAssemblyPath, tempFilePath, "ProductName",
                    versionResInformation.ProductName);

            if (versionResInformation.ProductVersion != null)
                ChangeResourceInformationProductVersion(tempAssemblyPath, tempFilePath, versionResInformation.ProductVersion);

            if (versionResInformation.AssemblyVersion != null)
                ChangeResourceInformationAttribute(tempAssemblyPath, tempFilePath, "Assembly Version",
                    versionResInformation.AssemblyVersion);

            // Original Filename is always last to prevent automatic overwriting
            if (versionResInformation.OriginalFilename != null)
                ChangeResourceInformationAttribute(tempAssemblyPath, tempFilePath, "OriginalFilename",
                    versionResInformation.OriginalFilename);

            assembly = AssemblyDef.Load(File.ReadAllBytes(tempAssemblyPath));
            
            File.Delete(tempFilePath); 
            File.Delete(tempAssemblyPath);
        }

        /// <summary>
        /// Changes the resource version information of an assembly
        /// </summary>
        /// <param name="file">File to modify</param>
        /// <param name="versionResInformation">AssemblyInformation to apply</param>
        public static void ChangeResourceInformation(string file, VersionResourceInformation versionResInformation)
        {
            string tempFilePath = DropVersionChanger();

            if (versionResInformation.CompanyName != null)
                ChangeResourceInformationAttribute(file, tempFilePath, "CompanyName",
                    versionResInformation.CompanyName);

            if (versionResInformation.FileDescription != null)
                ChangeResourceInformationAttribute(file, tempFilePath, "FileDescription",
                    versionResInformation.FileDescription);

            if (versionResInformation.FileVersion != null)
                ChangeResourceInformationFileVersion(file, tempFilePath, versionResInformation.FileVersion);

            if (versionResInformation.InternalName != null)
                ChangeResourceInformationAttribute(file, tempFilePath, "InternalName",
                    versionResInformation.InternalName);

            if (versionResInformation.LegalCopyright != null)
                ChangeResourceInformationAttribute(file, tempFilePath, "LegalCopyright",
                    versionResInformation.LegalCopyright);

            if (versionResInformation.ProductName != null)
                ChangeResourceInformationAttribute(file, tempFilePath, "ProductName",
                    versionResInformation.ProductName);

            if (versionResInformation.ProductVersion != null)
                ChangeResourceInformationProductVersion(file, tempFilePath, versionResInformation.ProductVersion);

            if (versionResInformation.AssemblyVersion != null)
                ChangeResourceInformationAttribute(file, tempFilePath, "Assembly Version",
                    versionResInformation.AssemblyVersion);

            // Original Filename is always last to prevent automatic overwriting
            if (versionResInformation.OriginalFilename != null)
                ChangeResourceInformationAttribute(file, tempFilePath, "OriginalFilename",
                    versionResInformation.OriginalFilename);

            File.Delete(tempFilePath);
        }

        /// <summary>
        /// Changes the value of a resource version information attribute
        /// </summary>
        /// <param name="tempAssemblyPath">Path to assembly</param>
        /// <param name="tempVersionChangerPath">Path to version changer executable</param>
        /// <param name="attribute">Name of attribute to modify</param>
        /// <param name="value">New value of attribute</param>
        public static void ChangeResourceInformationAttribute(string tempAssemblyPath, string tempVersionChangerPath, string attribute, string value)
        {
            var process = new Process();
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = tempVersionChangerPath;
            process.StartInfo.Arguments = string.Format("{0} /s \"{1}\" \"{2}\"", tempAssemblyPath, attribute, value);
            process.Start();
            process.WaitForExit();
        }

        /// <summary>
        /// Changes the resource version information file version
        /// </summary>
        /// <param name="tempAssemblyPath">Path to assembly</param>
        /// <param name="tempVersionChangerPath">Path to version changer executable</param>
        /// <param name="value">New value of attribute</param>
        public static void ChangeResourceInformationFileVersion(string tempAssemblyPath, string tempVersionChangerPath, string value)
        {
            var process = new Process();
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = tempVersionChangerPath;
            process.StartInfo.Arguments = string.Format("{0} \"{1}\"", tempAssemblyPath, value);
            process.Start();
            process.WaitForExit();
        }

        /// <summary>
        /// Changes the resource version information file version
        /// </summary>
        /// <param name="tempAssemblyPath">Path to assembly</param>
        /// <param name="tempVersionChangerPath">Path to version changer executable</param>
        /// <param name="value">New value of attribute</param>
        public static void ChangeResourceInformationProductVersion(string tempAssemblyPath, string tempVersionChangerPath, string value)
        {
            var process = new Process();
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = tempVersionChangerPath;
            process.StartInfo.Arguments = string.Format("{0} /pv \"{1}\"", tempAssemblyPath, value);
            process.Start();
            process.WaitForExit();
        }

        private static string DropVersionChanger()
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".exe");
            File.WriteAllBytes(tempFilePath, Properties.Resources.verpatch);

            return tempFilePath;
        }
    }
}