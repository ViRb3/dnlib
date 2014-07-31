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

using System.Collections.Generic;
using dnlib.DotNet;

namespace dnlib.Utils {
	/// <summary>
	/// Extension methods
	/// </summary>
	public static partial class Extensions {
        /// <summary>
        /// Returns the extended name of the type signature so to show full arguments
        /// </summary>
        /// <param name="typeSig">Type signature to process</param>
        /// <returns>The extended name</returns>
        public static string GetExtendedName(this TypeSig typeSig)
        {
            string name = typeSig.TypeName;

            if (typeSig.ToGenericInstSig() != null)
            {
                name += "<";

                IList<TypeSig> args = typeSig.ToGenericInstSig().GenericArguments;

                for (int i = 0; i < args.Count; i++)
                {
                    string newArgs = args[i].GetExtendedName();

                    if (newArgs != string.Empty)
                        name += newArgs;
                    else
                        name += args[i].TypeName;

                    if (i < args.Count - 1)
                        name += ", ";
                }

                name += ">";
            }

            return name;
        }

        /// <summary>
        /// Creates a list with all accessors of a type (Property methods, Event methods ...)
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
	}
}
