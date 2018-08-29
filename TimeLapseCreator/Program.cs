using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Timers;
namespace TimeLapseCreator
{
    class Program
    {
        public static bool IsProcessing = false;
        public static List<Camera> AllCameras = new List<Camera>();
        static void Main(string[] args)
        {
            try
            {
                /* Creating required Directories */

                if(!Directory.Exists(Constants.Logs_Directory))
                {
                    Directory.CreateDirectory(Constants.Logs_Directory);
                }
#if DEBUG
                 if (!Directory.Exists(Constants.OrgVedioDirectory_Debug))
                {
                    Directory.CreateDirectory(Constants.OrgVedioDirectory_Debug);
                }
                if (!Directory.Exists(Constants.TempVedioDirecory_Debug))
                {
                    Directory.CreateDirectory(Constants.TempVedioDirecory_Debug);
                }
#else
                if (!Directory.Exists(Constants.OrgVedioDirectory_Release))
                {
                    Directory.CreateDirectory(Constants.OrgVedioDirectory_Release);
                }
                if (!Directory.Exists(Constants.TempVedioDirecory_Release))
                {
                    Directory.CreateDirectory(Constants.TempVedioDirecory_Release);
                }
#endif
                /* Creating required Directories */


                /* Start Initializing Camera Objetcs */

                Camera tmp_CameraObj;
                /* Loading Camera IMSI numbers to create Camera objects with IMSI as constructor*/
                string ConfigFilePath = "";
#if DEBUG
                ConfigFilePath = Constants.ConfigurationFilePath_Debug;
#else
                ConfigFilePath = Constants.ConfigurationFilePath_Release;
#endif
                if (File.Exists(ConfigFilePath))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(ConfigFilePath);
                    XmlNodeList elem = doc.GetElementsByTagName("Camera");
                    foreach (XmlNode tag in elem)
                    {
                        tmp_CameraObj = new Camera(tag.Attributes["CameraIMSI"].Value);
                        AllCameras.Add(tmp_CameraObj);
                    }
                   
                }
                /* Loading Camera IMSI numbers End */
                /* End Initializing Camera Objects */

                /*Main Loop Start */
                foreach(Camera CameraObj in AllCameras)
                {
                    /* CheckVedioxistsInOriginalVedioFolder */

#if DEBUG
                      CameraObj.CheckVedioExistsInMainVedioFolderandUpdateProcessedVedioDate(Constants.OrgVedioDirectory_Debug);
#else
                       CameraObj.CheckVedioExistsInMainVedioFolderandUpdateProcessedVedioDate(Constants.OrgVedioDirectory_Release);
#endif
                    /* End CheckVedioxistsInOriginalVedioFolder*/

                        while ((DateTime.Compare(CameraObj.getProcessedVedioDate(),DateTime.Now.Date))<=0) /* while Processed Vedio Date <= TOday */
                        {
                            bool IsVideo_Created=CameraObj.MakeVedioOfProcessVedioDate();
                            if(IsVideo_Created)
                            {
                                CameraObj.PlaceVideoInMainVideoFolder();
                                CameraObj.DeleteProcessedVedioInTempFolder();
                            }
                            CameraObj.IncrementProcessedVedioDate(1);  

                        }
                    CameraObj.DecrementProcessedVedioDate(1); /* To Decrement processvideo date by 1 as it is incremeting 1 day un necessarly in above while loop*/
                }
                /* Main Loop End */

                Console.WriteLine("Application Is Running ......");
                Timer VedioUpdatingTimer;
                VedioUpdatingTimer = new Timer(Constants.VedioUpdationTime);  
                VedioUpdatingTimer.Elapsed += new ElapsedEventHandler(UpdateVediosOfAllCameras);
                VedioUpdatingTimer.AutoReset = true;
                VedioUpdatingTimer.Enabled = true;
                Console.ReadKey();
            }
            catch (Exception ex)
            {
            Global.AppendTexttoFile(Constants.ExceptionFilePath, ex.Message + "        " + DateTime.Now);
            }
       }
        /* End of  Static void main*/


        /* TImer Event Calling Function Start */
        private static void UpdateVediosOfAllCameras(object sender, ElapsedEventArgs elapsedEventArg)
        {
            
            try
            {
                if(!IsProcessing)
                {
                    /*Main Loop Start */
                    IsProcessing = true;
                    foreach (Camera CameraObj in AllCameras)
                    {

                        if(DateTime.Compare(CameraObj.getProcessedVedioDate(), DateTime.Now.Date)==0) /* If Processed Video Date is today only */
                        {
                            bool IsVideo_Created = CameraObj.MakeVedioOfProcessVedioDate();
                            if (IsVideo_Created)
                            {
                                CameraObj.PlaceVideoInMainVideoFolder();
                            }
                        }
                        else if(DateTime.Compare(CameraObj.getProcessedVedioDate(), DateTime.Now.Date)<0) /* Probably Yesterday  */
                        {
                            bool IsVideo_Created = CameraObj.MakeVedioOfProcessVedioDate();
                            if (IsVideo_Created)
                            {
                                CameraObj.PlaceVideoInMainVideoFolder();
                            }
                            CameraObj.IncrementProcessedVedioDate(1);
                            CameraObj.DeleteVideoOfDayBeforeProcessVideoDateInTempFolder(); // Because on day change video will be there in Temp FOlder 

                        }
                      
                    }
                    /* Main Loop End */
                    IsProcessing = false;
                }

                Console.WriteLine("Application is Running !.............");
                
            }
            catch(Exception ex)
            {
                Global.AppendTexttoFile(Constants.ExceptionFilePath, "Exception Occured In Method UpdateVediosOfAllCameras()   " + ex.Message + "            " + DateTime.Now.ToString());
                IsProcessing = false;
            }
        }
        /* TImer Event Calling Function End */
    }
}
