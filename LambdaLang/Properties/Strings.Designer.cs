﻿// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.17020
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace HL {
    using System;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static System.Resources.ResourceManager resourceMan;
        
        private static System.Globalization.CultureInfo resourceCulture;
        
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager {
            get {
                if (object.Equals(null, resourceMan)) {
                    System.Resources.ResourceManager temp = new System.Resources.ResourceManager("LambdaLang.Properties.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        internal static string Expression_ZeroDivisionError {
            get {
                return ResourceManager.GetString("Expression_ZeroDivisionError", resourceCulture);
            }
        }
        
        internal static string Expression_MaxRecursionDepth {
            get {
                return ResourceManager.GetString("Expression_MaxRecursionDepth", resourceCulture);
            }
        }
        
        internal static string Expression_Unexpected_symbol_Waits_for_comes {
            get {
                return ResourceManager.GetString("Expression_Unexpected_symbol_Waits_for_comes", resourceCulture);
            }
        }
        
        internal static string Cant_parse_table {
            get {
                return ResourceManager.GetString("Cant_parse_table", resourceCulture);
            }
        }
        
        internal static string Expression_Syntax_error {
            get {
                return ResourceManager.GetString("Expression_Syntax_error", resourceCulture);
            }
        }
    }
}
