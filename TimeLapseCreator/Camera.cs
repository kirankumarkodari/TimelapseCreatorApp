using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Video.FFMPEG;
using System.Drawing;
using System.Runtime.InteropServices;

namespace TimeLapseCreator
{
    public class Camera
    {
        string CameraIMSINumber;
        public DateTime ProcessVedioDate;/* ProcessVedioDate is for refering the which date vedio need to be created */
        public DateTime ProcessedVedioDate; /* ProcessedVedioDate is the date that vedio has been created */
        public long ProcessedImagetime; /* to store last processed image to optimize of Making video */
        public string RecentlyCreatedVideoName_Ogg;
        public string RecentlyCreatedVideoName_Mp4;
        public Camera(string CameraIMSINumber)
        {
            this.CameraIMSINumber = CameraIMSINumber;
        }
        public DateTime getProcessedVedioDate()
        {
            return this.ProcessVedioDate;
        }
        public void setProcessedVedioDate(DateTime ProcessedVedioDate)
        {
            this.ProcessVedioDate = ProcessedVedioDate;
        }
        public void IncrementProcessedVedioDate(Double NumberOfDays)
        {
            this.ProcessVedioDate= this.ProcessVedioDate.AddDays(NumberOfDays);  
        }
        public void DecrementProcessedVedioDate(Double NumberOfDays)
        {
            this.ProcessVedioDate = this.ProcessVedioDate.AddDays(-NumberOfDays);
        }
        public void CheckVedioExistsInMainVedioFolderandUpdateProcessedVedioDate(string VedioFilePath)
        {
        
            try
            {
                var directory = new DirectoryInfo(VedioFilePath);
                try
                {
                    var VedioFile = (from Vedio in directory.GetFiles("*" + this.CameraIMSINumber + Constants.VedioExtension_Ogg, SearchOption.AllDirectories)
                                     orderby Vedio.LastWriteTime descending
                                     select Vedio).First();

                    if (VedioFile == null)
                    {
                        /* No Vedio Exists */
                        this.ProcessVedioDate = DateTime.Now.Date.AddDays(-Constants.MaximumBackupDaysForVideo).Date; // to subtract the Maximum number of backup days from current datetime
                    }
                    else
                    {
                        /* Get Date from Vedio file name */
                        string VedioName = Path.GetFileNameWithoutExtension(VedioFile.FullName);
                        if (VedioName.Length == Constants.VedioFileNameLength)
                        {
                            
                            string DateStr = VedioName.Substring(Constants.StartIndexOfDateInVedioFileName, Constants.DateLength);
                            string DateStr_Format = DateStr.Substring(0, 4) + '/' + DateStr.Substring(4, 2) + '/' + DateStr.Substring(6, 2);
                            DateTime LastVedio_Date=Convert.ToDateTime(DateStr_Format);
                            this.ProcessVedioDate = LastVedio_Date.Date;
                            TimeSpan difference = DateTime.Now.Date - this.ProcessVedioDate;
                            int no_of_days_diff = (int)difference.TotalDays;
                            if (no_of_days_diff > Constants.MaximumBackupDaysForVideo)
                            {
                                int days_shouldadd = no_of_days_diff - Constants.MaximumBackupDaysForVideo;
                                this.ProcessVedioDate = this.ProcessVedioDate.AddDays(days_shouldadd);
                            }
                        }

                    }
                }
                catch(Exception ex)
                {
                    /* No Vedio Exists */
                    this.ProcessVedioDate = DateTime.Now.AddDays(-Constants.MaximumBackupDaysForVideo).Date;
                }
                
               
            }
            catch(Exception ex)
            {
                Global.AppendTexttoFile(Constants.ExceptionFilePath, "Exception Occured In CheckVedioExistsInMainVedioFolderandUpdateProcessedVedioDate ");
            }
        }
        public bool MakeVedioOfProcessVedioDate()
        {
            string VideoName = "";
            try
            {
                string day_str = Convert.ToString(this.ProcessVedioDate.Day);
                string month_str= Convert.ToString(this.ProcessVedioDate.Month);
                string year_str = Convert.ToString(this.ProcessVedioDate.Year);
                if(Convert.ToInt32(day_str)<10)
                {
                    day_str = "0" + day_str;
                }
                if(Convert.ToInt32(month_str)<10)
                {
                    month_str = "0" + month_str;
                }
                string ProcessVediodateStr = year_str + month_str + day_str;
                /* should so some work to frame it as 20170403 like that */
                VideoName = ProcessVediodateStr + this.CameraIMSINumber;
#if DEBUG
                var directory = new DirectoryInfo(Constants.ImagesDirectory_Debug);
#else
                var directory = new DirectoryInfo(Constants.ImagesDirectory_Release);
#endif

                FileInfo ImageFile=null;
                try
                {
                     ImageFile = (from Image in directory.GetFiles(ProcessVediodateStr + "*" + this.CameraIMSINumber + Constants.ImageExtension, SearchOption.AllDirectories)
                                     orderby Image.LastWriteTime descending
                                     select Image).First();
                }
               catch(Exception ex)
                {
                    /* no Image File exists for ProcessVediodateStr with that Cam IMSI */

                    return false; 
                }

                if (ImageFile == null)
                {
                    /* No Image Exists */
                    return false;
                }
                else
                {
                    /* Image Exists */
                    string ImageName = Path.GetFileNameWithoutExtension(ImageFile.FullName);

                    string DateTimeOfIMage = ImageName.Substring(0, 14);

                    long DateTimeOfImage_long = Convert.ToInt64(DateTimeOfIMage);

                    if(DateTimeOfImage_long>this.ProcessedImagetime) /* Recent Image is there we need to process it */
                    {
                        Bitmap tmp_bitmap;
                        using (VideoFileWriter VideoWriter_Ogg = new VideoFileWriter())
                        using(VideoFileWriter VideoWriter_Mp4 = new VideoFileWriter())
                        {
                            VideoWriter_Ogg.VideoCodec = VideoCodec.Theora;
                            VideoWriter_Ogg.Height = Constants.FrameHeight;
                            VideoWriter_Ogg.Width = Constants.FrameWidth;
                            VideoWriter_Ogg.FrameRate = Constants.FrameRate;
                           
                            VideoWriter_Mp4.VideoCodec = VideoCodec.H264;
                            VideoWriter_Mp4.Height = Constants.FrameHeight;
                            VideoWriter_Mp4.Width = Constants.FrameWidth;
                            VideoWriter_Mp4.FrameRate = Constants.FrameRate;
#if DEBUG
                            VideoWriter_Ogg.Open(Constants.TempVedioDirecory_Debug + VideoName + Constants.VedioExtension_Ogg); // Opening VideoFIle 
                            VideoWriter_Mp4.Open(Constants.TempVedioDirecory_Debug + VideoName + Constants.VedioExtension_mp4);
#else
                            VideoWriter_Ogg.Open(Constants.TempVedioDirecory_Release + VideoName + Constants.VedioExtension_Ogg); // Opening VideoFIle
                             VideoWriter_Mp4.Open(Constants.TempVedioDirecory_Release + VideoName + Constants.VedioExtension_mp4);
#endif
                            var AllFilesOfCamera = directory.GetFiles(ProcessVediodateStr + "*" + this.CameraIMSINumber + Constants.ImageExtension, SearchOption.AllDirectories);
                            foreach (var file in AllFilesOfCamera) /*To Iterate through All Files of that Camera */
                            {
                                string Imagefullpath = file.FullName;
                                if (this.IsValidImage(Imagefullpath))
                                {
                                    try
                                    {
                                        string tmp_Imagename = Path.GetFileNameWithoutExtension(Imagefullpath);
                                        string tmp_DateTimeOfIMage = tmp_Imagename.Substring(0, 14);
                                        long tmp_DateTimeOfImage_long = Convert.ToInt64(tmp_DateTimeOfIMage);
                                        this.ProcessedImagetime = tmp_DateTimeOfImage_long;
                                        string created_hourofImage_Str = tmp_Imagename.Substring(8, 2);
                                        short created_hourofImage_long = Convert.ToInt16(created_hourofImage_Str);
                                        if(created_hourofImage_long>=Constants.MinimumHourtoMakeVideo)
                                        {
                                            tmp_bitmap = ConvertToBitmap(Imagefullpath);
                                            VideoWriter_Ogg.WriteVideoFrame(tmp_bitmap);
                                            VideoWriter_Mp4.WriteVideoFrame(tmp_bitmap);
                                        }
                                        else
                                        {
                                            /* Don't need to make it in video  */
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Global.AppendTexttoFile(Constants.ExceptionFilePath, "Exception Occured While Writing Bitmap to Video frame  " + ex.Message + "            " + DateTime.Now.ToString());
                                    }

                                }
                            }
                            this.RecentlyCreatedVideoName_Ogg = VideoName + Constants.VedioExtension_Ogg;
                            this.RecentlyCreatedVideoName_Mp4 = VideoName + Constants.VedioExtension_mp4;
                            VideoWriter_Ogg.Close(); // TO CLose the VideoWriter After writing in to it..
                            VideoWriter_Mp4.Close();
                            this.ProcessedVedioDate = this.ProcessVedioDate;
                            return true;
                        }
                    }
                    else
                    {
                        /* No need of processing & creating video */
                        return false;
                    }
                    
                }
               
            }
            catch(Exception ex)
            {
                Global.AppendTexttoFile(Constants.ExceptionFilePath, "Exception Occured In Method MakeVedioOfProcessVedioDate()   " + ex.Message + "            " + DateTime.Now.ToString());
                if(VideoName!="")
                {
#if DEBUG
                    this.DeleteCorruptedVideoOfProcessVideoDate(Constants.TempVedioDirecory_Debug + VideoName + Constants.VedioExtension_Ogg);
                    this.DeleteCorruptedVideoOfProcessVideoDate(Constants.TempVedioDirecory_Debug + VideoName + Constants.VedioExtension_mp4);
#else
                    this.DeleteCorruptedVideoOfProcessVideoDate(Constants.TempVedioDirecory_Release + VideoName + Constants.VedioExtension_Ogg);
                    this.DeleteCorruptedVideoOfProcessVideoDate(Constants.TempVedioDirecory_Release + VideoName + Constants.VedioExtension_mp4);
#endif
                }
                
                return false;
            }
        }
        public void PlaceVideoInMainVideoFolder()
        {
            try
            {

#if DEBUG
                    string RecentlyCreatedtmpVideoPath_ogg=Constants.TempVedioDirecory_Debug+this.RecentlyCreatedVideoName_Ogg;
                    string TargetMainVideoFolderpath_ogg = Constants.OrgVedioDirectory_Debug + this.RecentlyCreatedVideoName_Ogg;
                    string RecentlyCreatedtmpVideoPath_mp4= Constants.TempVedioDirecory_Debug + this.RecentlyCreatedVideoName_Mp4;
                    string TargetMainVideoFolderpath_mp4 = Constants.OrgVedioDirectory_Debug + this.RecentlyCreatedVideoName_Mp4;
#else
                    string RecentlyCreatedtmpVideoPath_ogg=Constants.TempVedioDirecory_Release+this.RecentlyCreatedVideoName_Ogg;
                    string TargetMainVideoFolderpath_ogg = Constants.OrgVedioDirectory_Release + this.RecentlyCreatedVideoName_Ogg;
                    string RecentlyCreatedtmpVideoPath_mp4= Constants.TempVedioDirecory_Release + this.RecentlyCreatedVideoName_Mp4;
                    string TargetMainVideoFolderpath_mp4 = Constants.OrgVedioDirectory_Release + this.RecentlyCreatedVideoName_Mp4;
#endif

                /*    FileStream inf = new FileStream(RecentlyCreatedtmpVideoPath_mp4, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    FileStream outf = new FileStream(TargetMainVideoFolderpath_mp4, FileMode.Create);
                    int a;
                    while ((a = inf.ReadByte()) != -1)
                    {
                        outf.WriteByte((byte)a);
                    }
                    inf.Close();
                    inf.Dispose();
                    outf.Close();
                    outf.Dispose();
                    /*if (File.Exists(RecentlyCreatedtmpVideoPath_mp4))
                       {
                                File.Copy(RecentlyCreatedtmpVideoPath_mp4, TargetMainVideoFolderpath_mp4, true); // If the File Exists with the same name then need to ovverrite it (true)
                        }*/
                if (!(IsFileLocked(RecentlyCreatedtmpVideoPath_mp4)))
                {
                    
                    File.Copy(RecentlyCreatedtmpVideoPath_mp4, TargetMainVideoFolderpath_mp4,true); // If the File Exists with the same name then need to ovverrite it (true)
                  
                }

                if (!(IsFileLocked(RecentlyCreatedtmpVideoPath_ogg)))
                {
                   
                    File.Copy(RecentlyCreatedtmpVideoPath_ogg, TargetMainVideoFolderpath_ogg, true); // If the File Exists with the same name then need to ovverrite it (true)
                  
                }

            }
            catch(Exception ex)
            {
                Global.AppendTexttoFile(Constants.ExceptionFilePath, "Exception Occured In Method PlaceVideoInMainVideoFolder()   " + ex.Message + "            " + DateTime.Now.ToString());
            }

        }
        public bool IsFileLocked(string filePath)
        {
            try
            {
                using (File.Open(filePath, FileMode.Open,FileAccess.Read)) { }
            }
            catch (IOException e)
            {
                var errorCode = Marshal.GetHRForException(e) & ((1 << 16) - 1);

                return errorCode == 32 || errorCode == 33;
            }

            return false;
        }
        public void DeleteVideoOfDayBeforeProcessVideoDateInTempFolder() /* Delete video in tmp_videos folder */
        {
            try
            {
                DateTime tmp_ProcessedVideoDate = this.ProcessVedioDate;
                tmp_ProcessedVideoDate = tmp_ProcessedVideoDate.AddDays(-1);
                string day_str = Convert.ToString(tmp_ProcessedVideoDate.Day);
                string month_str = Convert.ToString(tmp_ProcessedVideoDate.Month);
                string year_str = Convert.ToString(tmp_ProcessedVideoDate.Year);

                if (Convert.ToInt32(day_str) < 10)
                {
                    day_str = "0" + day_str;
                }
                if (Convert.ToInt32(month_str) < 10)
                {
                    month_str = "0" + month_str;
                }
                string tmp_ProcessedVideoDate_str = year_str+ month_str+ day_str;
                string ProcessedVedio_name = tmp_ProcessedVideoDate_str + this.CameraIMSINumber;
#if DEBUG
                string ProcessedVedio_Path = Constants.TempVedioDirecory_Debug + ProcessedVedio_name + Constants.VedioExtension_Ogg;
                string ProcessedVedio_Path_Mp4 = Constants.TempVedioDirecory_Debug + ProcessedVedio_name + Constants.VedioExtension_mp4;
#else
                string ProcessedVedio_Path = Constants.TempVedioDirecory_Release + ProcessedVedio_name + Constants.VedioExtension_Ogg;
                string ProcessedVedio_Path_Mp4 = Constants.TempVedioDirecory_Release + ProcessedVedio_name + Constants.VedioExtension_mp4;
#endif
                if (File.Exists(ProcessedVedio_Path))
                {
                    File.Delete(ProcessedVedio_Path);
                }
                if (File.Exists(ProcessedVedio_Path_Mp4))
                {
                    File.Delete(ProcessedVedio_Path_Mp4);
                }

            }
            catch(Exception ex)
            {
                Global.AppendTexttoFile(Constants.ExceptionFilePath, "Exception Occured In Method DeleteVideoOfDayBeforeProcessVideoDate()   " + ex.Message + "            " + DateTime.Now.ToString());
            }
        }
        public void DeleteProcessedVedioInTempFolder()
        {
            try
            {
                DateTime tmp_ProcessedVideoDate = this.ProcessedVedioDate.Date;
                string day_str = Convert.ToString(tmp_ProcessedVideoDate.Day);
                string month_str = Convert.ToString(tmp_ProcessedVideoDate.Month);
                string year_str = Convert.ToString(tmp_ProcessedVideoDate.Year);

                if (Convert.ToInt32(day_str) < 10)
                {
                    day_str = "0" + day_str;
                }
                if (Convert.ToInt32(month_str) < 10)
                {
                    month_str = "0" + month_str;
                }
                string tmp_ProcessedVideoDate_str = year_str + month_str + day_str;
                string ProcessedVedio_name = tmp_ProcessedVideoDate_str + this.CameraIMSINumber;
#if DEBUG
                string ProcessedVedio_Path = Constants.TempVedioDirecory_Debug + ProcessedVedio_name + Constants.VedioExtension_Ogg;
                string ProcessedVedio_Path_Mp4 = Constants.TempVedioDirecory_Debug + ProcessedVedio_name + Constants.VedioExtension_mp4;
#else
                string ProcessedVedio_Path = Constants.TempVedioDirecory_Release + ProcessedVedio_name + Constants.VedioExtension_Ogg;
                string ProcessedVedio_Path_Mp4 = Constants.TempVedioDirecory_Release + ProcessedVedio_name + Constants.VedioExtension_mp4;
#endif
                if (File.Exists(ProcessedVedio_Path))
                {
                    File.Delete(ProcessedVedio_Path);
                }
                if (File.Exists(ProcessedVedio_Path_Mp4))
                {
                    File.Delete(ProcessedVedio_Path_Mp4);
                }

            }
            catch (Exception ex)
            {
                Global.AppendTexttoFile(Constants.ExceptionFilePath, "Exception Occured In Method DeleteProcessedVedioInTempFolder()   " + ex.Message + "            " + DateTime.Now.ToString());
            }
        }
        public void DeleteCorruptedVideoOfProcessVideoDate(string Videofile_path)
        {
            /* To Delete the video file whenever Exception occured while Making video */
            if(File.Exists(Videofile_path))
            {
                File.Delete(Videofile_path);
            }
            
        }
        public  bool IsValidImage(string full_file_path)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(full_file_path);
                byte first_one_byte = bytes[0];
                byte first_second_byte = bytes[1];
                byte last_second_byte = bytes[bytes.Length - 2];
                byte last_one_byte = bytes[bytes.Length - 1];  // Last One Byte 


                byte FF = 255;
                byte D8 = 216;
                byte D9 = 217;

                if ((first_one_byte.Equals(FF)) && (first_second_byte.Equals(D8)) && (last_one_byte.Equals(FF) || (last_second_byte.Equals(FF))))
                {
                    return true;
                }
                else
                {
                    return false;

                }
            }
            catch(Exception ex)
            {
                return false;
            }
            
        }
        public  Bitmap ConvertToBitmap(string fileName)
        {
            Bitmap bitmap;
            using (Stream bmpStream = System.IO.File.Open(fileName, System.IO.FileMode.Open))
            {
                Image image = Image.FromStream(bmpStream);

                bitmap = new Bitmap(image);

            }
            return bitmap;
        }
    }
}
