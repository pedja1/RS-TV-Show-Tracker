﻿namespace RoliSoft.TVShowTracker
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Net;
    using System.Numerics;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Contains informations about the assembly.
    /// </summary>
    public static partial class Signature
    {
        /// <summary>
        /// Gets the name of the software.
        /// </summary>
        /// <value>The software name.</value>
        public const string Software = "RS TV Show Tracker";

        /// <summary>
        /// Gets the name of the developer.
        /// </summary>
        /// <value>The developer name.</value>
        public const string Developer = "RoliSoft";

        /// <summary>
        /// Gets the version number of the executing assembly.
        /// </summary>
        /// <value>The software version.</value>
        public static string Version { get; private set; }

        /// <summary>
        /// Gets the formatted version number of the executing assembly.
        /// </summary>
        /// <value>The formatted software version.</value>
        public static string VersionFormatted { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this is a nightly build.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this is a nightly build; otherwise, <c>false</c>.
        /// </value>
        public static bool IsNightly
        {
            get
            {
                return
#if NIGHTLY
                    true
#else
                    false
#endif
;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this assembly has been compiled and signed by RoliSoft and has not been tampered with.
        /// </summary>
        /// <value><c>true</c> if this assembly has valid strong name; otherwise, <c>false</c>.</value>
        public static bool? IsOriginalAssembly { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a donation key has been entered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a donation key has been entered; otherwise, <c>false</c>.
        /// </value>
        public static bool IsActivated
        {
            get
            {
                return _isActivated;
            }
        }

        /// <summary>
        /// Gets a value indicating the status of the activation.
        /// </summary>
        /// <value>The activation status.</value>
        public static LicenseStatus ActivationStatus
        {
            get
            {
                return _licenseStatus;
            }
        }

        /// <summary>
        /// Gets the activation checksum.
        /// </summary>
        /// <value>The activation checksum.</value>
        public static string ActivationChecksum
        {
            get
            {
                return _licenseHash;
            }
        }

        /// <summary>
        /// Gets the full path to the executing assembly.
        /// </summary>
        /// <value>The full path to the executing assembly.</value>
        public static string InstallPath { get; private set; }

        /// <summary>
        /// Gets the full path to where UAC would virtualize the executing assembly.
        /// </summary>
        /// <value>The full path to where UAC would virtualize the executing assembly.</value>
        public static string UACVirtualPath { get; private set; }

        /// <summary>
        /// Gets the full path to the application data.
        /// </summary>
        /// <value>The full path to the application data.</value>
        public static string AppDataPath { get; private set; }

        /// <summary>
        /// This number is used for various purposes where a non-random unique number is required.
        /// </summary>
        public static long MagicNumber
        {
            get
            {
                return 0xFEEDFACEC0FFEE;
            }
        }

        /// <summary>
        /// Gets the Aperture Science Turret Testing Facility Access Codes.
        /// </summary>
        private static BigInteger[] Moduluses =
            {
                new BigInteger(15246487771235781107)*(17971768237329038849),
                new BigInteger(16455720859197541667)*(16693946381056259363),
                new BigInteger(14486207375898700283)*(16083913764276947789),
                new BigInteger(14283561927021492269)*(15127792701305930963),
                new BigInteger(14370260276245322459)*(16496639490075584219),
                new BigInteger(15914860067061659267)*(16579739426608955183),
                new BigInteger(14143463530261769267)*(14816180372324778737),
                new BigInteger(14726713035460975247)*(14922266034305327459),
                new BigInteger(15973096875095683133)*(17437565442450587429),
                new BigInteger(15028130840778560783)*(17277204033912866243),
            };

        /// <summary>
        /// Gets Cave Johnson's phone number in the form of an RSA key.
        /// </summary>
        private static RSAParameters PublicKey = new RSAParameters
            {
                Exponent = new byte[] { 0x01, 0x00, 0x01 },
                Modulus  = new byte[]
                    {
#region Big-ass modulus
                        0xCD, 0xBE, 0xB6, 0x22, 0xEF, 0x26, 0xFE, 0x8E, 0x5D, 0x4A, 0x78, 0xA8, 0x56, 0x07, 0x73, 0x4D,
                        0x71, 0xDF, 0x26, 0x17, 0x4D, 0x11, 0xD0, 0x42, 0xA9, 0xE1, 0xE2, 0xEC, 0xDF, 0x77, 0x92, 0x89,
                        0xE7, 0xBB, 0x4E, 0xF3, 0x1B, 0xBE, 0x9E, 0xB8, 0x63, 0x27, 0x10, 0xAC, 0xAB, 0xAD, 0xE5, 0x2D,
                        0x2D, 0xA8, 0xD6, 0x3C, 0x5A, 0xA0, 0xCE, 0xCA, 0x0F, 0xBA, 0x73, 0xAA, 0xB8, 0xF9, 0x3C, 0x1F,
                        0xB3, 0x7E, 0xE0, 0x82, 0x44, 0x71, 0xEB, 0x1E, 0x4F, 0xE4, 0xB4, 0x81, 0x31, 0x2E, 0x77, 0xE2,
                        0x58, 0x1E, 0x59, 0xE4, 0x0C, 0xFD, 0xB0, 0x27, 0x91, 0xC3, 0x8B, 0xD4, 0x62, 0x00, 0xCE, 0x77,
                        0x5D, 0x91, 0x24, 0xCA, 0x1E, 0x03, 0x1B, 0x08, 0x87, 0x5A, 0x1A, 0xF8, 0x72, 0xB0, 0xCE, 0x15,
                        0x97, 0x7A, 0x92, 0xD2, 0xAE, 0xD2, 0xB3, 0xEA, 0x7A, 0x24, 0x38, 0xB5, 0xCA, 0x41, 0xF9, 0x80,
                        0x61, 0xE1, 0xFB, 0x79, 0x48, 0x04, 0x46, 0x79, 0x6F, 0x69, 0x95, 0x38, 0xDD, 0xF8, 0xBC, 0xA0,
                        0xC4, 0xF3, 0xEF, 0x21, 0x8E, 0xFD, 0x59, 0xC7, 0xE0, 0xF2, 0xD1, 0xAC, 0x32, 0xB8, 0xFE, 0x5A,
                        0x4D, 0x4E, 0x76, 0x13, 0x63, 0x2E, 0x82, 0xE4, 0xD6, 0x8A, 0x5A, 0xAC, 0xAE, 0x20, 0xBB, 0x90,
                        0x3D, 0x98, 0x6D, 0x36, 0xD4, 0xC7, 0x08, 0xDE, 0x6D, 0x04, 0xFE, 0xCE, 0x07, 0x7D, 0xFB, 0x19,
                        0x3C, 0x50, 0xDC, 0xBA, 0x1D, 0xCB, 0x0C, 0x5A, 0xA0, 0xDB, 0x84, 0x77, 0x1A, 0x9D, 0xE0, 0x58,
                        0x8D, 0x93, 0xC1, 0xF7, 0x52, 0xCD, 0x8D, 0xB9, 0x03, 0x31, 0x5C, 0x6F, 0xA6, 0x9F, 0x95, 0x68,
                        0x1B, 0x54, 0xA4, 0xA7, 0xE9, 0x11, 0xF3, 0x4F, 0x64, 0x52, 0xEF, 0xE1, 0xFC, 0x56, 0xF1, 0x9A,
                        0x2A, 0x7F, 0xF2, 0x5F, 0x82, 0x24, 0xF0, 0x39, 0x3D, 0x81, 0x43, 0xA5, 0xAC, 0x11, 0xE1, 0x53,
                        0xEB, 0xFD, 0x4A, 0x3B, 0x69, 0x16, 0xCA, 0xB8, 0xDF, 0x50, 0xA6, 0x25, 0x09, 0xC6, 0x63, 0x3A,
                        0x92, 0xC4, 0x0D, 0x41, 0xBC, 0xBD, 0xF1, 0xB0, 0x83, 0x6A, 0xD9, 0xD2, 0x87, 0xB3, 0xD6, 0x82,
                        0xCA, 0xFA, 0x7C, 0x56, 0x38, 0x50, 0xC1, 0xFE, 0x78, 0x06, 0xBF, 0x44, 0x55, 0x2E, 0x4E, 0x84,
                        0x5A, 0x75, 0x96, 0xA7, 0xC2, 0x42, 0x16, 0xB7, 0x97, 0xD5, 0x7C, 0xCF, 0x4C, 0xE4, 0x9B, 0x69,
                        0xFB, 0x80, 0x52, 0x1C, 0xFE, 0xFA, 0xB1, 0x42, 0xED, 0x4B, 0xE4, 0xBB, 0x48, 0x8D, 0xC2, 0xE8,
                        0x19, 0x8B, 0x8D, 0x79, 0xA4, 0xBA, 0x78, 0xDE, 0x86, 0x4F, 0x84, 0x13, 0x7F, 0x3F, 0xDE, 0x88,
                        0x83, 0x7F, 0xBF, 0x83, 0x5D, 0x76, 0x60, 0x79, 0xCA, 0x5E, 0x3E, 0x3F, 0x8C, 0xB7, 0xFB, 0x43,
                        0x4E, 0x0B, 0x69, 0x6E, 0xCD, 0x0C, 0x27, 0xB7, 0xDD, 0xEF, 0x17, 0xE4, 0xCA, 0x37, 0x2B, 0xBA,
                        0x85, 0x42, 0x96, 0xF6, 0xAC, 0xD3, 0x97, 0x94, 0x4A, 0x86, 0x0F, 0x0A, 0xBC, 0x2B, 0xA2, 0x7F,
                        0x88, 0x8E, 0xBC, 0x16, 0xD5, 0x9B, 0x6A, 0xB6, 0xFE, 0xD4, 0x1B, 0x18, 0x5E, 0x20, 0xE8, 0x09,
                        0x1E, 0xA6, 0x1E, 0xF3, 0x72, 0x90, 0x23, 0x18, 0x42, 0x0B, 0x3A, 0x7B, 0x6F, 0xB4, 0x81, 0x40,
                        0x60, 0x44, 0x86, 0xF0, 0xD8, 0x4F, 0xF8, 0xF7, 0x96, 0xFF, 0x5E, 0xC7, 0x4A, 0x63, 0xC1, 0x08,
                        0xEF, 0xD9, 0x41, 0x5C, 0x49, 0x08, 0x55, 0xB2, 0xDE, 0xC1, 0x2F, 0x61, 0x8F, 0x2E, 0x8D, 0x4C,
                        0x20, 0xB0, 0x04, 0x2D, 0xE2, 0xAD, 0xB7, 0xDB, 0xF8, 0x0D, 0x00, 0x01, 0x9E, 0x6A, 0xE5, 0x3E,
                        0x41, 0x2A, 0x64, 0x4B, 0xC7, 0xAA, 0x5F, 0x69, 0x60, 0x6B, 0x4B, 0x7C, 0x08, 0x6C, 0xB2, 0x21,
                        0xF5, 0x82, 0xFD, 0xDE, 0xF6, 0x0F, 0x7D, 0x5D, 0xEA, 0xCA, 0x70, 0x36, 0xCA, 0x6E, 0x84, 0x86,
                        0x92, 0x3C, 0xD7, 0x94, 0x00, 0x61, 0x24, 0xB7, 0xEC, 0xF9, 0xC2, 0xBD, 0xFE, 0x0E, 0x3D, 0x9E,
                        0xF4, 0x75, 0x93, 0x71, 0x22, 0xA8, 0x52, 0xC7, 0x71, 0x8E, 0x9B, 0x7E, 0x2D, 0xBF, 0x49, 0x51,
                        0x14, 0x90, 0x66, 0x21, 0xFF, 0x30, 0x2F, 0x4E, 0xCA, 0x68, 0x45, 0x35, 0xF9, 0x52, 0xD0, 0x90,
                        0x2F, 0x81, 0x83, 0x54, 0x48, 0x42, 0x45, 0xB7, 0xEE, 0xB0, 0xD0, 0x95, 0xC2, 0x3E, 0x0C, 0xFB,
                        0xB9, 0x30, 0xFE, 0xE1, 0xE2, 0xC8, 0xC1, 0xA0, 0x0B, 0xC7, 0x44, 0xF1, 0x34, 0xCD, 0x61, 0x28,
                        0x4B, 0xE4, 0xAD, 0x39, 0x8A, 0x31, 0xF8, 0x60, 0xA2, 0x4D, 0x45, 0xF2, 0xEB, 0x7C, 0xFD, 0xA3,
                        0xD3, 0x5B, 0xD5, 0x80, 0xB8, 0xE5, 0x88, 0x84, 0xDC, 0xE1, 0x67, 0xC9, 0xCF, 0x88, 0x7F, 0x07,
                        0xF9, 0xAF, 0x78, 0x84, 0xF2, 0xA0, 0x19, 0xC0, 0x6F, 0x9B, 0x56, 0xB0, 0x1B, 0xF5, 0x48, 0x4F,
                        0x05, 0x5C, 0xC2, 0xA8, 0xD2, 0x5C, 0x9F, 0x4C, 0x28, 0x75, 0xCE, 0x3B, 0x5A, 0x43, 0xB0, 0xD2,
                        0x15, 0x01, 0xBD, 0x50, 0xFB, 0x9C, 0xF4, 0x76, 0x87, 0x36, 0x6A, 0x1C, 0x0E, 0xFD, 0xB3, 0xA3,
                        0xFC, 0xB8, 0x4E, 0x7E, 0x41, 0x9B, 0xE5, 0xE5, 0x1B, 0x8F, 0x0A, 0xEE, 0x53, 0x15, 0xC9, 0x07,
                        0x8B, 0xE1, 0x7B, 0xD7, 0x0D, 0xBE, 0xAD, 0x29, 0x44, 0x4A, 0xC6, 0x36, 0x59, 0x16, 0x7E, 0xE9,
                        0xF5, 0x88, 0xA7, 0x29, 0x42, 0x15, 0x4E, 0xC1, 0xD4, 0xEC, 0x90, 0xAC, 0x0D, 0x5C, 0xC7, 0x4D,
                        0x0F, 0x6B, 0xE4, 0xC2, 0xE8, 0x28, 0xA7, 0xA9, 0x8F, 0xE5, 0x29, 0xD3, 0xB1, 0x03, 0xC9, 0x5B,
                        0xE3, 0x36, 0x5E, 0x55, 0x68, 0xF5, 0x75, 0x1E, 0xB5, 0xD9, 0x5F, 0x85, 0xEE, 0x23, 0xD3, 0xC3,
                        0xE4, 0x28, 0x3E, 0x18, 0x61, 0xC6, 0xE0, 0x3E, 0xC9, 0x1C, 0xA9, 0xFD, 0xF7, 0xE0, 0x38, 0x69,
                        0x50, 0x92, 0xD4, 0xB5, 0x4D, 0xBB, 0xA2, 0x8E, 0xA5, 0x07, 0xE6, 0xA7, 0x0C, 0x01, 0xDB, 0x48,
                        0xD3, 0x9C, 0xD2, 0xB9, 0x31, 0x68, 0xFA, 0xD1, 0xB4, 0xDB, 0x5A, 0x26, 0xE9, 0x0D, 0x51, 0x8A,
                        0xB6, 0xE4, 0xED, 0xE1, 0x02, 0x5F, 0x75, 0xC9, 0x38, 0xCE, 0xDF, 0xB8, 0x7D, 0x40, 0x62, 0x21,
                        0x0A, 0x43, 0xE8, 0x3C, 0xEC, 0xFA, 0x82, 0xC0, 0xEE, 0x75, 0x10, 0x81, 0x88, 0xD3, 0xFE, 0x26,
                        0x96, 0x68, 0x07, 0x2A, 0x84, 0x9B, 0xB7, 0x3B, 0x70, 0x63, 0x59, 0x32, 0xAA, 0x0F, 0x51, 0xF0,
                        0x1B, 0x4F, 0x7A, 0xBB, 0x5E, 0x77, 0x0D, 0x08, 0x30, 0x59, 0x21, 0x79, 0xDF, 0xBF, 0xEE, 0x6D,
                        0x78, 0x90, 0xDC, 0x9E, 0xCC, 0xCC, 0xD5, 0xD4, 0xCE, 0x2A, 0xC5, 0x60, 0xDE, 0x6E, 0xD8, 0xFC,
                        0xAA, 0xB8, 0x52, 0x86, 0x1A, 0xB4, 0xC5, 0xEC, 0x2A, 0x23, 0x5D, 0x17, 0x2D, 0xE4, 0x32, 0x47,
                        0xBA, 0x3F, 0xFB, 0x59, 0x56, 0x59, 0x71, 0xE0, 0xD6, 0x03, 0xB1, 0x8C, 0xBA, 0xB2, 0xFE, 0x81,
                        0x1A, 0x24, 0x0F, 0x51, 0xEE, 0x9A, 0xBB, 0xF1, 0xAD, 0xD8, 0xA1, 0x34, 0x33, 0x69, 0x49, 0xDE,
                        0x48, 0x6F, 0xAB, 0x55, 0x33, 0xED, 0x6C, 0x42, 0x33, 0xAC, 0x2E, 0x51, 0x6E, 0xA5, 0x3B, 0x61,
                        0xFD, 0x3B, 0x5B, 0x5A, 0x54, 0xB5, 0x34, 0xCF, 0x5B, 0xA9, 0x3A, 0x2D, 0xC1, 0xA1, 0xA3, 0x77,
                        0x26, 0xC8, 0xB9, 0x1A, 0x85, 0x78, 0x59, 0xD5, 0xC3, 0x33, 0x37, 0x35, 0xA4, 0x54, 0xBA, 0xEE,
                        0xF5, 0xC7, 0x6C, 0xD6, 0xDC, 0x4D, 0x83, 0x7E, 0x45, 0x45, 0x1F, 0x20, 0x2D, 0x0B, 0x43, 0x3F,
                        0x95, 0x82, 0x5D, 0x69, 0xE4, 0xC8, 0x33, 0xF5, 0x08, 0x4D, 0x92, 0xA6, 0x10, 0xB7, 0xFA, 0x9C,
                        0x89, 0x86, 0xC4, 0x27, 0xD6, 0xFA, 0x9C, 0xEF, 0x9F, 0xC0, 0xF0, 0x5A, 0x3F, 0xBB, 0x15, 0xA4,
                        0x2C, 0x51, 0x43, 0x89, 0xC0, 0xE8, 0xFF, 0xEC, 0x5D, 0x30, 0x6F, 0x1C, 0x5D, 0xBD, 0x89, 0x89,
                        0x67, 0xD6, 0xF2, 0xCB, 0xFA, 0xA7, 0x6B, 0x26, 0x1A, 0x82, 0x18, 0x98, 0x6E, 0x4A, 0xB8, 0xEA,
                        0x94, 0x30, 0xF8, 0xDB, 0x67, 0x86, 0x6F, 0xB7, 0x34, 0xD6, 0x76, 0x7F, 0x27, 0x6E, 0x9A, 0xE3,
                        0x63, 0x40, 0x41, 0x24, 0xF4, 0x37, 0x75, 0xB0, 0xCA, 0x05, 0xDA, 0x97, 0x1F, 0xA6, 0x6A, 0x7E,
                        0xB9, 0xCB, 0x00, 0xC8, 0xA5, 0x70, 0xD3, 0xFB, 0x0F, 0xBF, 0x95, 0xFA, 0xD1, 0xC6, 0x00, 0x8C,
                        0x9C, 0xBE, 0xEB, 0x9D, 0x46, 0xE4, 0x3E, 0x6E, 0x22, 0x12, 0x9F, 0x7E, 0x33, 0x90, 0x74, 0xDE,
                        0x21, 0x73, 0x63, 0x24, 0xBB, 0x6B, 0x4B, 0x6C, 0x2C, 0xEA, 0xAE, 0xD7, 0xD6, 0x77, 0xF3, 0x06,
                        0x98, 0xCF, 0x08, 0x71, 0xC7, 0x89, 0xC6, 0xBC, 0xEC, 0x71, 0x19, 0xBE, 0xF8, 0xD2, 0x8A, 0xE5,
                        0x6A, 0x11, 0x56, 0x15, 0x9E, 0xD1, 0xDC, 0x47, 0x12, 0xA1, 0xFF, 0x2E, 0x37, 0x1A, 0x32, 0x78,
                        0xE7, 0xD2, 0x0A, 0x0B, 0x2C, 0xB0, 0x98, 0xA4, 0xA4, 0xDB, 0x32, 0xC3, 0x1E, 0xA9, 0xF4, 0x26,
                        0x26, 0xED, 0x98, 0xD8, 0x53, 0x88, 0x34, 0x79, 0x21, 0xA3, 0x20, 0x86, 0x1C, 0xB4, 0x0A, 0x3B,
                        0xC6, 0x63, 0x9C, 0x5F, 0xB7, 0x71, 0xAC, 0x1F, 0x62, 0xAD, 0xE3, 0x2D, 0x1F, 0x05, 0x36, 0x56,
                        0x2F, 0x97, 0x52, 0x50, 0x1A, 0xEC, 0x68, 0x08, 0xE3, 0x9A, 0x65, 0x61, 0x09, 0xCA, 0x24, 0x4F,
                        0x93, 0x1A, 0xA1, 0xE0, 0x73, 0x35, 0x82, 0x2F, 0x84, 0x6C, 0xA5, 0x0F, 0x03, 0xAF, 0x94, 0x6A,
                        0x92, 0xCD, 0xDF, 0x27, 0x72, 0xCD, 0x16, 0xC2, 0xD4, 0x6E, 0x34, 0x5A, 0x4E, 0x04, 0x1B, 0x86,
                        0x38, 0x48, 0x54, 0x82, 0xE6, 0x45, 0xFC, 0x02, 0xAF, 0x85, 0xD9, 0xAB, 0x0D, 0x58, 0x80, 0x3D,
                        0x67, 0x74, 0x21, 0xA8, 0x99, 0x45, 0x7B, 0xA9, 0x13, 0x57, 0x25, 0xA2, 0x33, 0x7F, 0x78, 0x92,
                        0xEB, 0xCD, 0x1A, 0xC8, 0xEC, 0x4F, 0x21, 0xCE, 0x04, 0x44, 0xA2, 0x73, 0xA0, 0xE7, 0xC7, 0xC9,
                        0xAE, 0x70, 0x40, 0x86, 0x62, 0x49, 0xF1, 0x84, 0x7F, 0x2E, 0x42, 0x3A, 0xBB, 0xAF, 0xBC, 0x60,
                        0xCE, 0xB8, 0x2A, 0xC0, 0x4C, 0xC8, 0xD4, 0x4A, 0x7B, 0xF2, 0x34, 0xF5, 0x91, 0x15, 0xCB, 0x8C,
                        0x70, 0xAF, 0x58, 0x96, 0xE4, 0x79, 0x29, 0xB2, 0x01, 0x0D, 0x91, 0x91, 0x2E, 0x08, 0xC0, 0x2E,
                        0xF7, 0xBF, 0x96, 0x16, 0x31, 0xF1, 0x00, 0xC8, 0xB8, 0x73, 0x17, 0x3A, 0xEE, 0x21, 0x9C, 0x77,
                        0xBA, 0x51, 0xEC, 0x4D, 0xF3, 0x48, 0x28, 0x18, 0xE0, 0xAD, 0x8E, 0x68, 0xE8, 0x52, 0xAF, 0x1B,
                        0xAD, 0xCD, 0x50, 0x37, 0xC3, 0xE8, 0x47, 0xD1, 0x66, 0x26, 0x97, 0x39, 0xB6, 0xB9, 0xE2, 0x58,
                        0x79, 0x8D, 0x53, 0x08, 0x47, 0x57, 0x91, 0xAA, 0x61, 0xE6, 0xCC, 0xCE, 0xE4, 0x2F, 0x47, 0xC3,
                        0xD2, 0xBF, 0x86, 0x39, 0x7A, 0x36, 0x3B, 0x0E, 0x0F, 0x5A, 0x06, 0x9E, 0xDB, 0x4F, 0xF5, 0x9C,
                        0xF9, 0x7F, 0x9C, 0x98, 0x44, 0x37, 0x97, 0xAE, 0xEB, 0xD7, 0x5C, 0x0E, 0xF8, 0x9D, 0x9B, 0xDB,
                        0x8B, 0x41, 0x7C, 0x5C, 0x92, 0x6E, 0x95, 0xD0, 0x0A, 0xCF, 0x39, 0x36, 0xAB, 0x08, 0x5B, 0x4A,
                        0xA5, 0xB5, 0x5A, 0x99, 0x82, 0x95, 0xEA, 0xA5, 0xE4, 0x09, 0x87, 0xE0, 0xE1, 0x76, 0x1F, 0xC3,
                        0x46, 0x3C, 0x37, 0xBC, 0xF6, 0xEB, 0x16, 0xDC, 0x4C, 0x79, 0x11, 0x6D, 0xE7, 0x73, 0xE6, 0x10,
                        0x8A, 0x49, 0xE8, 0x94, 0xAC, 0xFE, 0xB9, 0x06, 0x85, 0xFF, 0x11, 0xEF, 0x08, 0x12, 0xCF, 0x2E,
                        0xB0, 0x94, 0x9E, 0xDF, 0xB4, 0xDE, 0x28, 0x78, 0xD5, 0xB8, 0xE4, 0x13, 0x5B, 0x74, 0x11, 0xBD,
                        0xB5, 0x6B, 0x26, 0x37, 0x26, 0x94, 0x96, 0x31, 0xDD, 0x47, 0x0D, 0x10, 0x0C, 0x30, 0xE8, 0xD6,
                        0xD6, 0x8A, 0x55, 0x70, 0xC6, 0x8C, 0xB4, 0x76, 0x56, 0xBA, 0xC4, 0xFA, 0x16, 0x96, 0xCC, 0x81,
                        0x0C, 0xD0, 0xFC, 0x8E, 0xC1, 0x49, 0x07, 0xD3, 0xC4, 0x83, 0x3D, 0x7F, 0x22, 0xC3, 0x19, 0x2B,
                        0x30, 0x17, 0x01, 0x61, 0xAE, 0xC8, 0x58, 0x1E, 0x04, 0x88, 0xCC, 0xAD, 0x46, 0x53, 0xC0, 0x03,
                        0xC4, 0xD0, 0xB6, 0xA7, 0x20, 0xB8, 0x4D, 0x88, 0x6A, 0xE8, 0x28, 0xA1, 0xC4, 0x08, 0x55, 0x06,
                        0xDC, 0xDB, 0x42, 0x8A, 0x31, 0x8A, 0x1C, 0x91, 0x92, 0x26, 0x2E, 0xF1, 0x63, 0x92, 0xF9, 0x23,
                        0xCE, 0x1E, 0xCE, 0xA9, 0xBC, 0x69, 0x48, 0x3B, 0x7F, 0xFC, 0x77, 0xFB, 0x81, 0x7C, 0x96, 0x5E,
                        0x2B, 0x08, 0x3A, 0x0D, 0x8F, 0x03, 0x3C, 0xC6, 0x1D, 0x07, 0x8D, 0x7F, 0x45, 0xD0, 0xF8, 0xBC,
                        0xA3, 0xAB, 0x95, 0x51, 0x6D, 0x60, 0xFE, 0xB6, 0x29, 0x45, 0xC3, 0x55, 0x4F, 0x5F, 0x9C, 0x3C,
                        0x38, 0x56, 0x2C, 0x51, 0xE7, 0x75, 0x93, 0x92, 0x1A, 0x55, 0x21, 0x5D, 0x84, 0x90, 0x6E, 0xB8,
                        0x03, 0xE9, 0x07, 0x67, 0x2A, 0x1A, 0xD6, 0x5D, 0x20, 0x69, 0xBE, 0x1C, 0x0B, 0xB1, 0xC0, 0xD8,
                        0x62, 0x16, 0x8C, 0x07, 0x6F, 0x64, 0xDA, 0x8E, 0x99, 0x21, 0x1D, 0x0D, 0xFE, 0x84, 0x9A, 0xAE,
                        0xF8, 0xE7, 0x2C, 0x5F, 0xB1, 0x8A, 0x97, 0x66, 0x28, 0x79, 0xB4, 0xFD, 0xB6, 0xBC, 0x5E, 0x0A,
                        0x16, 0x6C, 0x81, 0x75, 0x5B, 0x0B, 0xAD, 0x06, 0xD1, 0x35, 0xCF, 0x41, 0xE6, 0x0A, 0xED, 0x63,
                        0x61, 0x7E, 0x26, 0xAF, 0xF7, 0x98, 0xA8, 0x9D, 0x76, 0xBD, 0x04, 0x71, 0x18, 0xED, 0x4F, 0xB3,
                        0x60, 0x8E, 0xE4, 0x49, 0xEE, 0x9E, 0x63, 0xA9, 0x43, 0x5D, 0xD3, 0x4D, 0x8E, 0xBD, 0xE4, 0xB5,
                        0x41, 0x95, 0xD2, 0xB9, 0x5F, 0x19, 0x23, 0x4A, 0x91, 0x41, 0x22, 0xE8, 0xCE, 0xFC, 0x5C, 0x89,
                        0xD5, 0xEB, 0xC2, 0xDF, 0xD3, 0x39, 0x07, 0xE9, 0x44, 0x07, 0xC4, 0x4E, 0xC3, 0x8F, 0x0C, 0xBC,
                        0x22, 0xA0, 0x12, 0xE4, 0x32, 0x83, 0xCC, 0xAB, 0xEE, 0x54, 0x58, 0x38, 0x68, 0x06, 0x30, 0xF7,
                        0x3D, 0x84, 0x92, 0x1D, 0x1A, 0xED, 0x59, 0xB7, 0xD4, 0x47, 0xB0, 0x88, 0x21, 0xB0, 0x68, 0x4A,
                        0x54, 0xE7, 0x32, 0xEC, 0xFE, 0x73, 0x3A, 0xA2, 0x10, 0x93, 0x2F, 0x75, 0x5D, 0x9B, 0xF6, 0xD5,
                        0xB0, 0x17, 0x26, 0x97, 0x6D, 0xDE, 0x50, 0xB5, 0xC8, 0xE6, 0xC1, 0xF9, 0x96, 0xA4, 0xFD, 0x87,
                        0xC3, 0x6D, 0x5C, 0xCF, 0x77, 0xEA, 0x14, 0xE5, 0x85, 0x01, 0x89, 0x03, 0x6A, 0x59, 0xAE, 0x0C,
                        0x09, 0xC6, 0xA6, 0x07, 0xC3, 0x11, 0xB9, 0x14, 0xA0, 0x41, 0xAC, 0x4A, 0x87, 0x1C, 0x10, 0xED,
                        0x8F, 0x10, 0xCE, 0x18, 0x50, 0x86, 0xA1, 0x96, 0x1E, 0x94, 0x9A, 0x1A, 0x8D, 0xA7, 0x54, 0x2B,
                        0x71, 0x74, 0x3A, 0x3F, 0x26, 0xD8, 0xA5, 0x5F, 0x70, 0xD3, 0x3B, 0xDB, 0x74, 0x57, 0xB9, 0x43,
                        0xAE, 0x75, 0x1E, 0x9C, 0xEE, 0x59, 0x90, 0x11, 0x9B, 0x95, 0x7B, 0x18, 0x2D, 0x80, 0x09, 0x93,
                        0xFC, 0x9B, 0x0E, 0xCF, 0x6D, 0x63, 0x85, 0x3F, 0x84, 0xC8, 0xE2, 0x63, 0x90, 0xCD, 0x63, 0xA9,
                        0x18, 0xB1, 0x19, 0x89, 0xE3, 0x6E, 0x78, 0xAD, 0x65, 0x70, 0x00, 0x0E, 0x38, 0x3B, 0x91, 0x12,
                        0x75, 0xF8, 0xEA, 0x05, 0x56, 0x9F, 0xCF, 0xDD, 0xEB, 0x57, 0x2F, 0xED, 0x2F, 0x22, 0xBF, 0x48,
                        0xD5, 0x7A, 0x66, 0x43, 0x33, 0xDB, 0x20, 0x80, 0xC8, 0x2F, 0x47, 0xCB, 0xC1, 0x32, 0x89, 0x89,
                        0x8C, 0xB7, 0xDE, 0xDA, 0xF9, 0x8E, 0x14, 0x06, 0x6A, 0x21, 0x5B, 0x24, 0x9A, 0x9E, 0xB8, 0x2F,
#endregion
                    }
            };

        private static bool _isActivated;
        private static LicenseStatus _licenseStatus = LicenseStatus.Uninitialized;
        private static string _licenseHash = null;

        /// <summary>
        /// Initializes static members of the <see cref="Signature"/> class. 
        /// </summary>
        static Signature()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;

            Version = ver.Major + "." + ver.Minor + "." + ver.Build + "." + ver.Revision;
            VersionFormatted = "v" + ver.Major + "." + ver.Minor + (ver.Build != 0 ? "." + ver.Build : string.Empty) + " b" + ver.Revision + (IsNightly ? " nightly" : string.Empty);

            Log.Info(Software + " " + VersionFormatted);
            Log.Info("Compiled on " + BuildMachine + " at " + CompileTime.ToString("yyyy'-'MM'-'dd HH':'mm':'ss") + " from commit " + GitRevision);

            try
            {
                AppDataPath    = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Developer, Software) + Path.DirectorySeparatorChar;
                InstallPath    = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar;
                UACVirtualPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualStore", Regex.Replace(InstallPath, @"^[a-zA-Z]{1,2}:[\\/]", string.Empty));
            }
            catch (ArgumentException ex)
            {
                Log.Warn("Error while loading $AppDataPath/$InstallPath/$UACVirtualPath.", ex);
            }

            if (AppDataPath != null && !Directory.Exists(AppDataPath))
            {
                try
                {
                    Directory.CreateDirectory(AppDataPath);
                }
                catch (Exception ex)
                {
                    Log.Error("$AppDataPath (" + AppDataPath + ") doesn't exist and can't be created.", ex);
                }
            }

            int minWorkThd = 0, maxWorkThd = 0, minIOWait = 0, maxIOWait = 0, cpu = Environment.ProcessorCount, destCpu = cpu * 8;
            ThreadPool.GetMinThreads(out minWorkThd, out minIOWait);
            ThreadPool.GetMaxThreads(out maxWorkThd, out maxIOWait);

            Log.Debug("Thread pool status: " + minWorkThd + "/" + minIOWait + " min, " + maxWorkThd + "/" + maxIOWait + " max, " + cpu + " cpu.");

            if (ThreadPool.SetMinThreads(destCpu, destCpu))
            {
                Log.Debug("Thread pool minimum set to " + destCpu + " threads.");
            }
            else
            {
                Log.Warn("Failed to set thread pool minimum to " + destCpu + " threads.");
            }

            Task.Factory.StartNew(() =>
                {
                    var snToken = Assembly.GetExecutingAssembly().GetName().GetPublicKeyToken();

                    if (snToken == null || snToken.Length == 0)
                    {
                        IsOriginalAssembly = false;
                        Log.Warn("Assembly strong name is missing.");
                        return;
                    }

                    var x = 0;
                    if (!snToken.All(b => Math.Abs(b - Math.Round(Math.Ceiling(Math.Abs(48.9089619634291 * Math.Max(Math.Tan(x), x) - 145.506677297553)) - Math.Tan(-32.1977538252671 * Math.Max(Math.Max(-1.617542499971, -Math.Round(Math.Tan(x))), 49.1817299 * (x % 7) - 216.296680292888)))) <= Math.Abs(x - ++x)))
                    {
                        IsOriginalAssembly = false;
                        Log.Warn("Assembly strong name is present but not original key.");
                        return;
                    }

                    bool pfWasVerified = false, result;

                    try
                    {
                        result = Utils.Interop.StrongNameSignatureVerificationEx(Assembly.GetExecutingAssembly().Location, true, ref pfWasVerified);
                    }
                    catch (Exception ex)
                    {
                        IsOriginalAssembly = false;
                        Log.Warn("Error while verifying assembly strong name signature.", ex);
                        return;
                    }

                    if (result && pfWasVerified)
                    {
                        IsOriginalAssembly = true;
                        Log.Debug("Assembly strong name is present and valid.");
                    }
                    else if (result)
                    {
                        IsOriginalAssembly = false;
                        Log.Warn("Assembly strong name is present but was not verified.");
                    }
                    else
                    {
                        IsOriginalAssembly = false;
                        Log.Warn("Assembly strong name is present but invalid.");
                    }

                    InitLicense();
                });
        }

        /// <summary>
        /// Gets the numbers. This is an easter egg. ;)
        /// </summary>
        /// <returns>
        /// The numbers.
        /// </returns>
        public static IEnumerable<int> GetNumbers()
        {
            for (var x = 1; x != 6; x++)
            {
                yield return (int)(60 + 4.25 * Math.Pow(x * x, 2) + 91.75 * x * x - 29.375 * x * Math.Pow(x, 2) - 0.22499999 * x * Math.Pow(x, 2) * Math.Pow(x, 2) - 122.4 * x);
            }
        }

        /// <summary>
        /// Gathers basic computer information which is then later used to generate an unique ID for machine key.
        /// </summary>
        public static Dictionary<string, List<Dictionary<string, string>>> GetComputerInfo()
        {
            var infos = new Dictionary<string, List<Dictionary<string, string>>>
                {
                    { "MBD", new List<Dictionary<string, string>>() },
                    { "CPU", new List<Dictionary<string, string>>() },
                    { "RAM", new List<Dictionary<string, string>>() },
                    { "HDD", new List<Dictionary<string, string>>() },
                    { "PRT", new List<Dictionary<string, string>>() },
                    { "GPU", new List<Dictionary<string, string>>() },
                };

            try
            {
                var mos = new ManagementObjectSearcher("select Manufacturer, Product, SerialNumber from Win32_BaseBoard").Get();

                foreach (var mo in mos)
                {
                    var inf = new Dictionary<string, string>
                        {
                            { "Company", string.Empty },
                            { "Model",   string.Empty },
                            { "Serial",  string.Empty },
                        };

                    try { inf["Company"] = mo["Manufacturer"].ToString(); } catch { }
                    try { inf["Model"]   = mo["Product"].ToString();      } catch { }
                    try { inf["Serial"]  = mo["SerialNumber"].ToString(); } catch { }

                    infos["MBD"].Add(inf);
                }
            }
            catch { }

            try
            { 
                var mos = new ManagementObjectSearcher("select DeviceID, Name, Caption, MaxClockSpeed, ProcessorId from Win32_Processor").Get();

                foreach (var mo in mos)
                {
                    var inf = new Dictionary<string, string>
                        {
                            { "ID",          string.Empty },
                            { "Processor",   string.Empty },
                            { "Description", string.Empty },
                            { "Speed",       string.Empty },
                            { "Serial",      string.Empty },
                        };

                    try { inf["ID"]          = mo["DeviceID"].ToString();      } catch { }
                    try { inf["Processor"]   = mo["Name"].ToString();          } catch { }
                    try { inf["Description"] = mo["Caption"].ToString();       } catch { }
                    try { inf["Speed"]       = mo["MaxClockSpeed"].ToString(); } catch { }
                    try { inf["Serial"]      = mo["ProcessorId"].ToString();   } catch { }

                    infos["CPU"].Add(inf);
                }
            }
            catch { }

            try
            {
                var mos = new ManagementObjectSearcher("select DeviceID, StartingAddress, EndingAddress from Win32_MemoryDevice").Get();

                foreach (var mo in mos)
                {
                    var inf = new Dictionary<string, string>
                        {
                            { "ID",    string.Empty },
                            { "Start", string.Empty },
                            { "End",   string.Empty },
                        };

                    try { inf["ID"]    = mo["DeviceID"].ToString();        } catch { }
                    try { inf["Start"] = mo["StartingAddress"].ToString(); } catch { }
                    try { inf["End"]   = mo["EndingAddress"].ToString();   } catch { }

                    infos["RAM"].Add(inf);
                }
            }
            catch { }
            
            try
            {
                var mos = new ManagementObjectSearcher("select DeviceID, Model, Size, SerialNumber from Win32_DiskDrive where MediaType = \"Fixed hard disk media\"").Get();

                foreach (var mo in mos)
                {
                    var inf = new Dictionary<string, string>
                        {
                            { "ID",     string.Empty },
                            { "Drive",  string.Empty },
                            { "Size",   string.Empty },
                            { "Serial", string.Empty },
                        };

                    try { inf["ID"]     = mo["DeviceID"].ToString();     } catch { }
                    try { inf["Drive"]  = mo["Model"].ToString();        } catch { }
                    try { inf["Size"]   = mo["Size"].ToString();         } catch { }
                    try { inf["Serial"] = mo["SerialNumber"].ToString(); } catch { }

                    infos["HDD"].Add(inf);
                }
            }
            catch { }

            try
            {
                var mos = new ManagementObjectSearcher("select DeviceID, FileSystem, Size, VolumeSerialNumber from Win32_LogicalDisk where DriveType = 3").Get();

                foreach (var mo in mos)
                {
                    var inf = new Dictionary<string, string>
                        {
                            { "ID",     string.Empty },
                            { "Type",   string.Empty },
                            { "Size",   string.Empty },
                            { "Serial", string.Empty },
                        };

                    try { inf["ID"]     = mo["DeviceID"].ToString();           } catch { }
                    try { inf["Type"]   = mo["FileSystem"].ToString();         } catch { }
                    try { inf["Size"]   = mo["Size"].ToString();               } catch { }
                    try { inf["Serial"] = mo["VolumeSerialNumber"].ToString(); } catch { }

                    infos["PRT"].Add(inf);
                }
            }
            catch { }

            try
            {
                var mos = new ManagementObjectSearcher("select DeviceID, Name from Win32_VideoController").Get();

                foreach (var mo in mos)
                {
                    var inf = new Dictionary<string, string>
                        {
                            { "ID",    string.Empty },
                            { "Serie", string.Empty },
                        };

                    try { inf["ID"]    = mo["DeviceID"].ToString(); } catch { }
                    try { inf["Serie"] = mo["Name"].ToString();     } catch { }

                    infos["GPU"].Add(inf);
                }
            }
            catch { }

            return infos;
        }

        /// <summary>
        /// Initializes the license.
        /// </summary>
        public static void InitLicense()
        {
            _isActivated = false;
            _licenseHash = null;

            if (!IsOriginalAssembly.GetValueOrDefault())
            {
                _licenseStatus = LicenseStatus.Aborted;
                Log.Warn("License initialization aborted due to assembly tampering.");
                return;
            }

            var licfile = Path.Combine(AppDataPath, ".license");

            if (!File.Exists(licfile))
            {
                _licenseStatus = LicenseStatus.NotAvailable;
                Log.Debug("Licensing data does not exist.");
                return;
            }

            Log.Debug("Loading licensing information...");

            try
            {
                string user, key;
                byte[] xkey, lic;

                using (var fs = File.OpenRead(licfile))
                using (var br = new BinaryReader(fs))
                {
                    var status = br.ReadByte();

                    if (status > 200)
                    {
                        _licenseStatus = LicenseStatus.KeyStatusError;
                        Log.Error("The license is cryptographically valid, but has been denied by the activation server. Your key was most likely revoked. Please contact rolisoft@gmail.com for more information.");
                        return;
                    }

                    user = br.ReadString();
                    xkey = br.ReadBytes(br.Read7BitEncodedInt());
                    lic  = br.ReadBytes(br.Read7BitEncodedInt());
                }

                try
                {
                    xkey = ProtectedData.Unprotect(xkey, null, DataProtectionScope.LocalMachine);
                    lic  = ProtectedData.Unprotect(lic, null, DataProtectionScope.LocalMachine);
                    key  = Encoding.UTF8.GetString(xkey);
                }
                catch
                {
                    _licenseStatus = LicenseStatus.LicenseDecryptError;
                    Log.Error("The license is not for this computer or Windows machine keys were reset.");
                    return;
                }

                if (!VerifyKey(user, key))
                {
                    _licenseStatus = LicenseStatus.KeyCryptoError;
                    Log.Error("The key within the license is cryptographically invalid. If you have recently updated the software, the keying scheme might have been changed. If you did not receive an email containing a new key, please contact rolisoft@gmail.com for more information.");
                    return;
                }

                var mbdsn = string.Empty;

                try
                {
                    var mos = new ManagementObjectSearcher("select SerialNumber from Win32_BaseBoard").Get();

                    foreach (var mo in mos)
                    {
                        mbdsn = mo["SerialNumber"].ToString();
                        break;
                    }
                }
                catch { }

                var cpusn = string.Empty;

                try
                {
                    var mos = new ManagementObjectSearcher("select ProcessorId from Win32_Processor").Get();

                    foreach (var mo in mos)
                    {
                        cpusn = mo["ProcessorId"].ToString();
                        break;
                    }
                }
                catch { }

                var hddsn = string.Empty;

                try
                {
                    var mos = new ManagementObjectSearcher("select SerialNumber from Win32_DiskDrive where MediaType = \"Fixed hard disk media\"").Get();

                    foreach (var mo in mos)
                    {
                        hddsn = mo["SerialNumber"].ToString();
                        break;
                    }
                }
                catch { }

                var prtsn = string.Empty;

                try
                {
                    var mos = new ManagementObjectSearcher("select VolumeSerialNumber from Win32_LogicalDisk where DriveType = 3").Get();

                    foreach (var mo in mos)
                    {
                        prtsn = mo["VolumeSerialNumber"].ToString();
                        break;
                    }
                }
                catch { }

                var verify = Encoding.UTF8.GetBytes(user + '\0' + key + '\0' + mbdsn + '\0' + cpusn + '\0' + hddsn + '\0' + prtsn);

                using (var rsa = new RSACryptoServiceProvider(0))
                {
                    rsa.ImportParameters(PublicKey);

                    _isActivated = rsa.VerifyData(verify, SHA512.Create(), lic);

                    if (_isActivated)
                    {
                        _licenseHash = BitConverter.ToString(new HMACSHA512(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(user.ToLower().Trim()))).ComputeHash(Encoding.UTF8.GetBytes(key.Trim()))).ToLower().Replace("-", string.Empty);
                        Log.Info("License validated for " + user + ". Thank you for supporting the software!");
                    }
                    else
                    {
                        _licenseStatus = LicenseStatus.LicenseInvalid;
                        Log.Error("The license is not for this computer or hardware S/Ns have changed recently.");
                    }
                }
            }
            catch (Exception ex)
            {
                _licenseStatus = LicenseStatus.LicenseException;
                Log.Error("Exception occurred during license validation.", ex);
            }
        }

        /// <summary>
        /// Saves the license.
        /// </summary>
        public static void SaveLicense(string user, string key, byte[] license)
        {
            var licfile = Path.Combine(AppDataPath, ".license");

            var xkey = ProtectedData.Protect(Encoding.UTF8.GetBytes(key), null, DataProtectionScope.LocalMachine);
            var lic  = ProtectedData.Protect(license, null, DataProtectionScope.LocalMachine);
            
            using (var fs = File.Create(licfile))
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write((byte)Utils.Rand.Next(0, 200));
                bw.Write(user);
                bw.Write7BitEncodedInt(xkey.Length);
                bw.Write(xkey);
                bw.Write7BitEncodedInt(lic.Length);
                bw.Write(lic);
            }
        }

        /// <summary>
        /// Checks the response of the REST API.
        /// </summary>
        /// <param name="resp">The response.</param>
        public static void CheckAPIResponse(HttpWebResponse resp)
        {
            if (!_isActivated)
            {
                return;
            }

            var kst = KeyStatus.Unchecked;

            try { kst = (KeyStatus)int.Parse(resp.Headers["X-Key"]); } catch { }

            if (kst != KeyStatus.Valid && kst != KeyStatus.Unchecked)
            {
                _isActivated   = false;
                _licenseHash   = null;
                _licenseStatus = LicenseStatus.KeyStatusError;

                try
                {
                    using (var fs = File.OpenWrite(Path.Combine(AppDataPath, ".license")))
                    {
                        fs.Position = 0;
                        fs.WriteByte((byte)((int)kst + 200));
                        fs.Flush(true);
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Determines whether the specified donation key is cryptographically valid.
        /// </summary>
        /// <param name="user">The email address of the user.</param>
        /// <param name="key">The corresponding donation key.</param>
        /// <returns>
        ///   <c>true</c> if the verification was successful; otherwise, <c>false</c>.
        /// </returns>
        public static bool VerifyKey(string user, string key)
        {
            var a = Encoding.UTF8.GetBytes(user.ToLower().Trim());
            var b = new BigInteger(new HMACSHA512(MD5.Create().ComputeHash(a)).ComputeHash(a).Truncate(16).Reverse().ToArray());
            var c = key.Trim().Replace("-", string.Empty).Substring(2).Reverse().ToList();
            var d = (c.Aggregate(int.Parse(key.TrimStart().Substring(0, 2), NumberStyles.HexNumber), (i, x) => i - x) - 5 * 45) & byte.MaxValue;
            var e = Enumerable.Range('0', '9' - '0' + 1).Concat(Enumerable.Range('a', 'z' - 'a' + 1)).Concat(Enumerable.Range('A', 'Z' - 'A' + 1)).Select(x => (char)x).ToList();
            var f = c.Aggregate(BigInteger.Zero, (g, i, x) => BigInteger.Add(g, BigInteger.Multiply(e.IndexOf(x), BigInteger.Pow(e.Count, i))));
            return Moduluses.Any(m => BigInteger.Subtract(b, BigInteger.ModPow(f, 0x010001, m)).IsZero) && 0 == d;
        }

        /// <summary>
        /// Checks whether the specified donation key is active on the server. 
        /// </summary>
        /// <param name="user">The email address of the user.</param>
        /// <param name="key">The corresponding donation key.</param>
        /// <returns>
        /// The key status returned by the server.
        /// </returns>
        public static KeyStatus CheckKey(string user, string key)
        {
            var hash   = BitConverter.ToString(new HMACSHA512(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(user.ToLower().Trim()))).ComputeHash(Encoding.UTF8.GetBytes(user.Trim()))).ToLower().Replace("-", string.Empty);
            var result = Remote.API.CheckDonateKey(hash);

            if (!result.Success)
            {
                return KeyStatus.Unchecked;
            }

            return (KeyStatus)result.Result;
        }

        /// <summary>
        /// A list of donation key statuses.
        /// </summary>
        public enum KeyStatus : int
        {
            /// <summary>
            /// There was an error while preforming the check.
            /// </summary>
            Unchecked = -1,

            /// <summary>
            /// The key is valid and active.
            /// </summary>
            Valid = 0,

            /// <summary>
            /// The key is not registered on the server, and it is not even cryptographically valid.
            /// </summary>
            Invalid = 1,

            /// <summary>
            /// The key is not registered on the server, however it is cryptographically valid.
            /// </summary>
            Unrecognized = 2,

            /// <summary>
            /// The key was erroneously issued, and therefore it was revoked.
            /// </summary>
            Revoked = 3,

            /// <summary>
            /// The key was suspended, possibly due to suspicious activity.
            /// </summary>
            Suspended = 4,

            /// <summary>
            /// The key was disabled, possibly due to not following the one and only rule: don't fuckin' share it.
            /// </summary>
            Disabled = 5
        }

        /// <summary>
        /// A list of license statuses.
        /// </summary>
        public enum LicenseStatus : int
        {
            /// <summary>
            /// The license was not yet initialized.
            /// </summary>
            Uninitialized = -1,

            /// <summary>
            /// The license is valid and active.
            /// </summary>
            Valid = 0,

            /// <summary>
            /// No donation key was specified.
            /// </summary>
            NotAvailable = 2,

            /// <summary>
            /// The specified donation key is cryptographically invalid.
            /// </summary>
            KeyCryptoError = 3,

            /// <summary>
            /// The specified donation key is cryptographically valid but deemed invalid by the activation server.
            /// </summary>
            KeyStatusError = 4,

            /// <summary>
            /// A call to <c>ProtectedData.Unprotect()</c> failed. License was not encrypted here.
            /// </summary>
            LicenseDecryptError = 5,

            /// <summary>
            /// There was an exception while loading the license file, it is most likely corrupt.
            /// </summary>
            LicenseException = 6,

            /// <summary>
            /// The RSA signature verification of the license failed.
            /// </summary>
            LicenseInvalid = 7,

            /// <summary>
            /// The cryptographic signature of the assembly is invalid.
            /// </summary>
            Aborted = 8
        }
    }
}
