using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VideoOS.Platform.SDK.UI.LoginDialog;

namespace SimpleVideoViewer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            VideoOS.Platform.SDK.Environment.Initialize();          // General initialize.  Always required
            VideoOS.Platform.SDK.UI.Environment.Initialize();       // Initialize UI references
                                                                    //VideoOS.Platform.SDK.Export.Environment.Initialize();	// Initialize export references

            DialogLoginForm loginForm = new DialogLoginForm(SetLoginResult);
            //loginForm.LoginLogoImage = MyOwnImage;				// Set own header image
            Application.Run(loginForm);                             // Show and complete the form and login to server
            if (Connected)
            {
                Application.Run(new MainForm());
            }

        }

        private static bool Connected = false;
        private static void SetLoginResult(bool connected)
        {
            Connected = connected;
        }

    }
}
