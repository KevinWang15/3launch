using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;

namespace _3launch
{
    class Util
    {
        /*
         * convert from key to char (only supports A~Z, a~z, 0~9)
         */
        public static char keyToChar(Key key)
        {
            var str = key.ToString();
            if (str.Length != 1)
            {
                if (str.Length == 2 && str[0] == 'D')
                {
                    var chr = str[1];
                    if (chr >= '0' && chr <= '9')
                        return chr;
                }
                if (str.StartsWith("NumPad"))
                {
                    return str[6];
                }
                return '\0';
            }
            else
            {
                return str[0];
            }
        }

        public static bool isKeyCodeValid(int keyCode)
        {
            return ((keyCode >= 'a' && keyCode <= 'z') ||
                    (keyCode >= 'A' && keyCode <= 'Z') ||
                    (keyCode >= '0' && keyCode <= '9'));
        }

        public static string GetSystemDefaultBrowser()
        {
            string name = string.Empty;
            RegistryKey regKey = null;

            try
            {
                //set the registry key we want to open
                regKey = Registry.ClassesRoot.OpenSubKey("HTTP\\shell\\open\\command", false);

                //get rid of the enclosing quotes
                name = regKey.GetValue(null).ToString().ToLower().Replace("" + (char) 34, "");

                //check to see if the value ends with .exe (this way we can remove any command line arguments)
                if (!name.EndsWith("exe"))
                    //get rid of all command line arguments (anything after the .exe must go)
                    name = name.Substring(0, name.LastIndexOf(".exe") + 4);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                //check and see if the key is still open, if so
                //then close it
                if (regKey != null)
                    regKey.Close();
            }
            //return the value
            return name;
        }
    }
}