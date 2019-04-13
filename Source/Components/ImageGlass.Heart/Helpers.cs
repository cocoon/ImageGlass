﻿/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2019 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


using ImageMagick;
using System;
using System.IO;
using System.Linq;

namespace ImageGlass.Heart
{
    public class Helpers
    {
        /// <summary>
        /// Preprocess MagickImage data
        /// </summary>
        /// <param name="img">IMagickImage data</param>
        /// <param name="colorProfileName">Name or Full path of color profile</param>
        /// <param name="isApplyColorProfileForAll">If FALSE, only the images with embedded profile will be applied</param>
        /// <returns></returns>
        public static IMagickImage PreprocessMagickImage(IMagickImage img, string colorProfileName = "sRGB", bool isApplyColorProfileForAll = false)
        {
            img.Quality = 100;

            //Get Exif information
            var profile = img.GetExifProfile();
            if (profile != null)
            {
                //Get Orieantation Flag
                var exifTag = profile.GetValue(ExifTag.Orientation);

                if (exifTag != null)
                {
                    int orientationFlag = int.Parse(profile.GetValue(ExifTag.Orientation).Value.ToString());

                    var orientationDegree = GetOrientationDegree(orientationFlag);
                    if (orientationDegree != 0)
                    {
                        //Rotate image accordingly
                        img.Rotate(orientationDegree);
                    }
                }
            }


            // get the color profile of image
            var imgColorProfile = img.GetColorProfile();


            // if always apply color profile
            // or only apply color profile if there is an embedded profile
            if (isApplyColorProfileForAll || imgColorProfile != null)
            {
                if (imgColorProfile != null)
                {
                    // correct the image color space
                    img.ColorSpace = imgColorProfile.ColorSpace;
                }
                else
                {
                    // set default color profile and color space
                    img.AddProfile(ColorProfile.SRGB);
                    img.ColorSpace = ColorProfile.SRGB.ColorSpace;
                }

                var colorProfile = GetColorProfile(colorProfileName);
                if (colorProfile != null)
                {
                    img.AddProfile(colorProfile);
                    img.ColorSpace = colorProfile.ColorSpace;
                }
            }

            return img;
        }


        /// <summary>
        /// Get built-in color profiles
        /// </summary>
        /// <returns></returns>
        public static string[] GetBuiltInColorProfiles()
        {
            return new string[]
            {
                "AdobeRGB1998",
                "AppleRGB",
                "CoatedFOGRA39",
                "ColorMatchRGB",
                "sRGB",
                "USWebCoatedSWOP",
            };
        }


        /// <summary>
        /// Get the correct color profile name
        /// </summary>
        /// <param name="name">Name or Full path of color profile</param>
        /// <returns></returns>
        public static string GetCorrectColorProfileName(string name)
        {
            var profileName = "";

            if (File.Exists(name))
            {
                return name;
            }
            else
            {
                var builtInProfiles = GetBuiltInColorProfiles();
                var result = builtInProfiles.FirstOrDefault(i => i.ToUpperInvariant() == name.ToUpperInvariant());

                if (result != null)
                {
                    profileName = result;
                }
                else
                {
                    return string.Empty;
                }
            }

            return profileName;
        }



        #region PRIVATE FUNCTIONS

        /// <summary>
        /// Get ColorProfile
        /// </summary>
        /// <param name="name">Name or Full path of color profile</param>
        /// <returns></returns>
        private static ColorProfile GetColorProfile(string name)
        {
            if (File.Exists(name))
            {
                return new ColorProfile(name);
            }
            else
            {
                // get all profile names in Magick.NET
                var profiles = typeof(ColorProfile).GetProperties();
                var result = profiles.FirstOrDefault(i => i.Name.ToUpperInvariant() == name.ToUpperInvariant());

                if (result != null)
                {
                    try
                    {
                        return (ColorProfile)result.GetValue(result);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }

            return null;
        }



        /// <summary>
        /// Returns Exif rotation in degrees. Returns 0 if the metadata 
        /// does not exist or could not be read. A negative value means
        /// the image needs to be mirrored about the vertical axis.
        /// </summary>
        /// <param name="orientationFlag">Orientation Flag</param>
        /// <returns></returns>
        private static double GetOrientationDegree(int orientationFlag)
        {
            if (orientationFlag == 1)
                return 0;
            else if (orientationFlag == 2)
                return -360;
            else if (orientationFlag == 3)
                return 180;
            else if (orientationFlag == 4)
                return -180;
            else if (orientationFlag == 5)
                return -90;
            else if (orientationFlag == 6)
                return 90;
            else if (orientationFlag == 7)
                return -270;
            else if (orientationFlag == 8)
                return 270;

            return 0;
        }


        #endregion
    }
}
