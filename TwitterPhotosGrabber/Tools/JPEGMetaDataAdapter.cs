using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace TwitterPhotosGrabber.Tools
{
    class JPEGMetaDataAdapter
    {
        private readonly string path;
        private BitmapFrame frame;
        public readonly BitmapMetadata Metadata;

        public JPEGMetaDataAdapter(string path)
        {
            try
            {
                this.path = path;
                this.frame = getBitmapFrame(path);
                Metadata = (BitmapMetadata)frame.Metadata.Clone();
            }
            catch (Exception e)
            {
                Utils.WriteLine(e);
            }
        }

        public void Save()
        {
            SaveAs(path);
        }

        public void SaveAs(string path)
        {
            try
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(frame, frame.Thumbnail, Metadata, frame.ColorContexts));
                using (Stream stream = File.Open(path, FileMode.Create, FileAccess.ReadWrite))
                {
                    encoder.Save(stream);
                }
            }
            catch (Exception e)
            {
                Utils.WriteLine(e);
            }
        }


        private BitmapFrame getBitmapFrame(string path)
        {
            try
            {
                BitmapDecoder decoder = null;
                using (Stream stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                }
                return decoder.Frames[0];
            }
            catch (Exception e)
            {
                Utils.WriteLine(e);
                return null;
            }
        }
    }
}
