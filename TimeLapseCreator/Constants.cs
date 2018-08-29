using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLapseCreator
{
    public static class Constants
    {
            public const short MaximumBackupDaysForVideo = 4; // 4 days
            public const int VedioUpdationTime = 1000 * 60 * 60; // 1 hour 
           // public const int VedioUpdationTime = 1000 * 60 ; // 1 minute 
            public const short NumberOfCameras = 4; // 4 cameras;
        public const short MinimumHourtoMakeVideo = 6;
        /*  TempVedio Directory */
            public const string TempVedioDirecory_Debug = @"D:\Timelapse_Vedios\Temp\";
            public const string TempVedioDirecory_Release = @"D:\IOT\RWM\WMS DeploymentRD\Timelapse_Vedios\Temp\";
        /*   End TempVedio Directory */
        /*   OriginalVedio Directory */
            public const string OrgVedioDirectory_Debug = @"D:\Timelapse_Vedios\";
            public const string OrgVedioDirectory_Release = @"D:\IOT\RWM\WMS DeploymentRD\Timelapse_Vedios\";
        /*   OriginalVedio Directory */
        /*    Images Directory */
            public const string ImagesDirectory_Debug = @"D:\CamImages\";
            public const string ImagesDirectory_Release = @"D:\IOT\RWM\WMS DeploymentRD\OnlineImages\";
            public const string ImageExtension = ".jpg";
            public const string VedioExtension_mp4 = ".mp4";
            public const string VedioExtension_Ogg = ".ogg";
            public const short VedioFileNameLength = 24;
            public const short ImageFileNameLength = 40;
            public const short StartIndexOfCameraIMSIInVedioFileName = 8;
            public const short StartIndexOfCameraIMSIInImageFileName = 24;
            public const short CameraIMSINumberLength = 16;
            public const short StartIndexOfDateInVedioFileName = 0;
            public const short StartIndexOfDateInImageFileName = 0;
            public const short DateLength = 8;
            
        /*    End Images Directory */
            public const int FrameWidth = 2048;
            public const int FrameHeight = 1536;
            public const int FrameRate = 3;
            public const int BitRate = 1000000;
            public const string Logs_Directory= @"C:\VedioMakingSoftware\";
            public const string ExceptionFilePath = @"C:\VedioMakingSoftware\Exceptions.txt";
            public const string LogsFilePath = @"C:\VedioMakingSoftware\Logs.txt";
               /* Camera 1 */
            public const string CameraIMSINumber1_Debug = "0405854056261389";
            public const string CameraIMSINumber1_Release = "0405854056013886";
            /* Camera 2 */
            public const string CameraIMSINumber2_Debug = "0405854056013904";
            public const string CameraIMSINumber2_Release = "0405854056013904";
            /* Camera 3 */
            public const string CameraIMSINumber3_Debug = "0405854056013902";
            public const string CameraIMSINumber3_Release = "0405854056013902";
            /* Camera 4 */
            public const string CameraIMSINumber4_Debug = "0405854056013886";
            public const string CameraIMSINumber4_Release = "0405854056013886";

            public const string ConfigurationFilePath_Debug = @"D:\Configdata.xml";
            public const string ConfigurationFilePath_Release = @"D:\IOT\RWM\WMS DeploymentRD\Configurations\Configdata.xml";
    }
}
