using LionvilleSystems.DataManagerCommonClasses;
using LionvilleSystems.La7CartBase;
using LionvilleSystems.La7UsbClr;
using LionvilleSystems.LockAlertCommonClasses;
using SecureRxDatabaseXML;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CartTest
{
    public partial class NetLa71Main : Form
    {
        private CheckBox[] UnlockedLeds;
        private CheckBox[] ToUnlock;
        private CheckBox[] OpenedLeds;

        public int[] DrawerMasks = new int[]
        {
	        16,
	        32,
	        64,
	        128,
	        256,
	        512,
	        1024,
	        2048,
	        4096,
	        8192,
	        16384,
	        32768,
	        65536,
	        131072,
	        262144,
	        524288,
	        1048576,
	        2097152,
	        4194304,
	        8388608,
	        16777216,
	        33554432,
	        67108864,
	        134217728
        };

        public CCartConfig CartConfig;
        public CLa7DevStatus La7Status;

        public int CsLockServoMask;
        public int CsDwrServoMask;
        public int CartDrawerMask;

        public CLa7CartBase CartBase;

        public bool bDevicePresent;
        public bool bCartConnected;

        public const int MainLimitSwMask = 4;
        public const int MainLockSwMask = 8;
        public const int MF_BYPOSITION = 0x400;
        public const int MF_STRING = 0;

        public const int WM_CYCLE_PORT = 0x420;
        public const int WM_DEVICECHANGE = 0x219;
        public const int WM_SET_DEMO = 0x460;
        public const int WM_SIM_ERROR = 0x430;
        public const int WM_SYSCOMMAND = 0x112;
        public const int WM_WTSSESSION_CHANGE = 0x2b1;


        public const int WM_DEVELOPER = 0x450;
        public const int WM_LIFETEST = 0x410;

        private int uCsMask;

        private int CurrentSwitches;
        private int CurrentUnlocked;

        public NetLa71Main()
        {

            InitializeComponent();

            this.UnlockedLeds = new CheckBox[] { 
                this.chkUnlockedLed1, this.chkUnlockedLed2, this.chkUnlockedLed3, this.chkUnlockedLed4, 
                this.chkUnlockedLed5, this.chkUnlockedLed6, this.chkUnlockedLed7, this.chkUnlockedLed8, 
                this.chkUnlockedLed9, this.chkUnlockedLed10, this.chkUnlockedLed11, this.chkUnlockedLed2,
                this.chkUnlockedLed13, this.chkUnlockedLed14, this.chkUnlockedLed15, this.chkUnlockedLed16, 
                this.chkUnlockedLed17, this.chkUnlockedLed18, this.chkUnlockedLed19, this.chkUnlockedLed20,
                this.chkUnlockedLed21, this.chkUnlockedLed22, this.chkUnlockedLed23, this.chkUnlockedLed24
             };

            this.ToUnlock = new CheckBox[] { 
                this.chkToUnlock1, this.chkToUnlock2, this.chkToUnlock3, this.chkToUnlock4, this.chkToUnlock5, this.chkToUnlock6, this.chkToUnlock7, this.chkToUnlock8, this.chkToUnlock9, this.chkToUnlock10, this.chkToUnlock11, this.chkToUnlock12, this.chkToUnlock13, this.chkToUnlock14, this.chkToUnlock15, this.chkToUnlock16, 
                this.chkToUnlock17, this.chkToUnlock18, this.chkToUnlock19, this.chkToUnlock20, this.chkToUnlock21, this.chkToUnlock22, this.chkToUnlock23, this.chkToUnlock24
             };

            this.OpenedLeds = new CheckBox[]  { 
                this.chkOpenedLed1, this.chkOpenedLed2, this.chkOpenedLed3, this.chkOpenedLed4, this.chkOpenedLed5, this.chkOpenedLed6, this.chkOpenedLed7, 
                this.chkOpenedLed8, this.chkOpenedLed9, this.chkOpenedLed10, this.chkOpenedLed11, this.chkOpenedLed12, this.chkOpenedLed12, this.chkOpenedLed14, this.chkOpenedLed15, this.chkOpenedLed16, 
                this.chkOpenedLed17, this.chkOpenedLed18, this.chkOpenedLed19, this.chkOpenedLed20, this.chkOpenedLed21, this.chkOpenedLed22, 
                this.chkOpenedLed23, this.chkOpenedLed24
             };
        }

        private void NetLa71Main_Load(object sender, EventArgs e)
        {
            string str;

            GlobalSettings.InitGlobalSettings(GlobalsFiles.SecureRx);
            LocalSettings.InitLocalSettings(LocalsFiles.SecureRx);
            int intLocalSetting = LocalSettings.GetIntLocalSetting(LocalSettings.STR_LS_DataIdx);
            Encryption.bEncrypt = (intLocalSetting != 0);

            //while (!this.OpenDatabase())
            //{
            //    if (MessageBox1.Show(this, Resources.WouldYouLikeToTryToRecoverABackedUpCopyOfTheDatabase, initializeApplication, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            //    {
            //        try
            //        {
            //            SecureRxDb.SaveCorruptDatabase();
            //            new RestoreDbDlg(this).ShowDialog();
            //        }
            //        catch (Exception exception)
            //        {
            //            str = string.Format(Resources.ErrorBackingUpTheCorruptDatabaseFilesNX0, exception.Message);
            //            MessageBox1.Show(this, str, initializeApplication, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //            this.WriteErrorLog("NetLa71Main", sFunc, ErrorLogTypes.Database, initializeApplication, str);
            //        }
            //    }
            //    else
            //    {
            //        base.Close();
            //        return;
            //    }
            //}
            while (!this.GetCartConfig())
            {
                //if (MessageBox1.Show(this, Resources.WouldYouLikeToTryToRecoverABackedUpCopyOfTheDatabase, initializeApplication, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                //{
                //    try
                //    {
                //        SecureRxDb.SaveCorruptDatabase();
                //        new RestoreDbDlg(this).ShowDialog();
                //    }
                //    catch (Exception exception2)
                //    {
                //        str = string.Format(Resources.ErrorBackingUpTheCorruptDatabaseFilesNX0, exception2.Message);
                //        MessageBox1.Show(this, str, initializeApplication, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //        this.WriteErrorLog("NetLa71Main", sFunc, ErrorLogTypes.Database, initializeApplication, str);
                //    }
                //}
                //else
                //{
                //    base.Close();
                //    return;
                //}
            }
            try
            {
                //SecureRxDb.BackupDatabaseFiles();
            }
            catch (Exception exception3)
            {
                //str = string.Format(Resources.ErrorBackingUpTheDatabaseFilesNX0, exception3.Message);
                //this.WriteErrorLog("NetLa71Main", sFunc, ErrorLogTypes.Database, initializeApplication, str);
            }
            this.CartBase = new CLa7CartBase();
            //this.CartBase.OnError = new LionvilleSystems.La7UsbClr.ErrorHandler(this.ErrorHandler);
            this.CartBase.OnDeviceStatus = new DeviceStatusHandler(this.La7StatusHandler);
            //this.CartBase.OnFirmwareVersion = new FirmwareVersionHandler(this.VersionHandler);
            this.CartBase.OnDeviceRemoved = new LionvilleSystems.La7UsbClr.DeviceRemovedHandler(this.DeviceRemovedHandler);
            this.CartBase.OnStartPolling = new StartPollingDoneHandler(this.StartPollDoneHandler);
            this.CartBase.OnMainPulseTimes = new LionvilleSystems.La7UsbClr.MainPulseTimesHandler(this.MainPulseTimesHandler);
            //this.CartBase.WriteErrorLog = new WriteErrorLogDelegate(this.WriteErrorLog);

           

            str = this.CartBase.InitializeCartBase();
            if (str != null)
            {
                //str = string.Format(Resources.CantInitializeTheCartBaseNX0, str);
                //MessageBox1.Show(this, str, initializeApplication, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //this.WriteErrorLog("NetLa71Main", sFunc, ErrorLogTypes.Controller, initializeApplication, str);
                //base.Close();
            }
            this.bDevicePresent = this.CartBase.DeviceIsPresent();
            this.ProcessConnection(this.bDevicePresent);
            this.StartConnectionTimer(true);
            this.SetServoPulseWidths();
            this.SetMotorRunTimes();


            this.CartBase.DemoFlag = false;

            this.BuildCsServoMasks(true);

            this.uCsMask = (ushort)this.CsLockServoMask;
            try
            {
                this.CartBase.SetStatusPolling(true);
            }
            catch (La7UsbException exception)
            {
                //string sMsg = string.Format(Resources.ErrorStartingDevicePollingNX0, exception.Message);
                //string signingIn = Resources.SigningIn;
                //MessageBox1.Show(sMsg, signingIn, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //Main.WriteErrorLog("ServiceDlg", "ServiceDlg", ErrorLogTypes.Controller, signingIn, sMsg);
            }

            
        }

        private void NetLa71Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (this.bLoggingInitialized)
            //{
            //    AuditLogging.LogMiscAuditEvent(ActivityCodes.LOG_LA71_SHUT_DOWN, true);
            //}

            try
            {
                this.CartBase.LockAllServos();
            }
            catch (Exception exception)
            {
                //this.Main.WriteErrorLog("CartOperationDlg", "CartOperationDlg_FormClosing", ErrorLogTypes.Controller, Resources.NotDisplayedToUser, exception.Message);
            }

            if (this.CartBase != null)
            {
                this.CartBase.ShutDownCartBase();

            }
        }


        private void DeviceRemovedHandler()
        {
            this.bDevicePresent = false;
            this.bCartConnected = false;
            base.BeginInvoke(new System.Windows.Forms.MethodInvoker(delegate
            {
                this.ProcessConnection(this.bDevicePresent);
                //his.StartConnectionTimer(this.bUserLoggedIn);
            }));
        }

        private void La7StatusHandler(CLa7DevStatus Status)
        {
            this.La7Status = Status;
            base.BeginInvoke(new System.Windows.Forms.MethodInvoker(delegate
            {
                this.ProcessDeviceStatus(Status);
            }));
        }

        private void StartPollDoneHandler(bool bErr, string sMessage)
        {
            base.BeginInvoke(new System.Windows.Forms.MethodInvoker(delegate
            {
                this.ProcessStartPollDone(bErr, sMessage);
            }));
        }
        public void ProcessStartPollDone(bool bErr, string sMessage)
        {
            if (bErr)
            {
                //string sMsg = string.Format(Resources.ErrorStartingDevicePollingNX0, sMessage);
                //string signingIn = Resources.SigningIn;
                //MessageBox1.Show(this, sMsg, signingIn, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //this.Main.WriteErrorLog("ServiceDlg", "ProcessStartPollDone", ErrorLogTypes.Controller, signingIn, sMsg);
            }
            else
            {
                this.bCartConnected = true;
            }
        }

        private void MainPulseTimesHandler(double dUnlockTime, double dLockTime)
        {
            LocalSettings.SetLocalSetting(LocalSettings.STR_LS_MainUnlockTime, dUnlockTime.ToString("F3"));
            LocalSettings.SetLocalSetting(LocalSettings.STR_LS_MainLockTime, dLockTime.ToString("F3"));
        }


        private bool GetCartConfig()
        {
            //this.CurCartID = GlobalSettings.GetStringGlobalSetting(GlobalsFiles.SecureRx, GlobalSettings.STR_GS_LocalCartID);
            try
            {
                this.CartConfig = CartConfigDb.GetSecureRxCart();
                if (this.CartConfig == null)
                {
                    this.CartConfig = new CCartConfig();
                    //this.CartConfig.CartID = this.CurCartID;
                    //his.CartConfig.CartName = Resources.LockAlertCart;
                    this.CartConfig.AuditLogEnabled = true;
                    this.CartConfig.CartFlags = 0x10;
                    CartConfigDb.AddCartRecord(this.CartConfig);
                    //this.AddDefaultLogins();
                }
            }
            catch (Exception exception)
            {
                //string sMsg = string.Format(Resources.ErrorAccessingCartConfigRecoreNX0, exception.Message);
                //string getCartConfig = Resources.GetCartConfig;
                //MessageBox1.Show(this, sMsg, getCartConfig, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //this.WriteErrorLog("NetLa71Main", "GetCartConfig", ErrorLogTypes.Database, getCartConfig, sMsg);
                return false;
            }
            return true;
        }

        private void btnUnlockDwrs_Click(object sender, EventArgs e)
        {
            int selectedServos = this.GetSelectedServos();
            int drawerMask = selectedServos & this.CsLockServoMask;
            selectedServos &= ~drawerMask;
            try
            {
                if (selectedServos != 0)
                {
                    this.CartBase.UnlockDrawers(selectedServos);
                }
                if (drawerMask != 0)
                {
                    this.CartBase.UnlockCsServos(drawerMask);
                }
            }
            catch (La7UsbException exception)
            {
                //string sMsg = string.Format(Resources.ErrorUnlockingSelectedServosNX0, exception.Message);
                //string unlockSelected = Resources.UnlockSelected;
                //MessageBox.Show(this, sMsg, unlockSelected, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //this.Main.WriteErrorLog("CartOperationDlg", "btnUnlockDwrs_Click", ErrorLogTypes.Controller, unlockSelected, sMsg);
                MessageBox.Show(exception.Message);
            }
            //AuditLogging.LogDrawersEvent(ActivityCodes.LOG_SVC_UNLOCK_LA71_DWRS, selectedServos);
        }

        public int GetSelectedServos()
        {
            int num = 0;
            for (int i = 0; i < this.ToUnlock.Length; i++)
            {
                if (this.ToUnlock[i].Checked)
                {
                    num |= this.DrawerMasks[i];
                }
            }
            return num;
        }

        public void BuildCsServoMasks(bool bCheckConfig)
        {
            List<CDrawer> drawersForCart = null;
            List<CDrawer> list2 = null;
            int lockMask = 0;
            int dwrMask = 0;
            int num3 = 0;
            ushort num4 = 0;
            string sMsg = null;
            bool flag = false;
            try
            {
                drawersForCart = DrawerDb.GetDrawersForCart(this.CartConfig.CartLink);
                list2 = new List<CDrawer>(8);
                if ((drawersForCart != null) && (drawersForCart.Count != 0))
                {
                    foreach (CDrawer drawer in drawersForCart)
                    {
                        num3 |= drawer.DrawerServo;
                        if (drawer.CsServo != 0)
                        {
                            list2.Add(drawer);
                            lockMask |= drawer.CsServo;
                            dwrMask |= drawer.DrawerServo;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                //sMsg = string.Format(Resources.ErrorGettingDrawerRecordsFromDatabaseNX0, exception.Message);
            }
            if (sMsg == null)
            {
                this.CsLockServoMask = lockMask;
                this.CsDwrServoMask = dwrMask;
                this.CartDrawerMask = num3;
                try
                {
                    this.CartBase.SetCsServoMasks(lockMask, dwrMask);
                }
                catch (La7UsbException exception2)
                {
                    //sMsg = string.Format(Resources.ErrorSendingCsMasksToControllerNX0, exception2.Message);
                    sMsg = exception2.Message;
                    flag = true;
                }
                if ((sMsg == null) && bCheckConfig)
                {
                    for (int i = 0; i < list2.Count; i++)
                    {
                        num4 = (ushort)(num4 | ((ushort)this.DrawerMasks[i]));
                    }
                    if (num4 != this.CartConfig.CSConfigMask)
                    {
                        this.CartConfig.CSConfigMask = num4;
                        try
                        {
                            CartConfigDb.UpdateCartRecord(this.CartConfig);
                        }
                        catch (Exception exception3)
                        {
                            //sMsg = string.Format(Resources.ErrorUpdatingTheCartConfigurationNX0, exception3.Message);
                            sMsg = exception3.Message;
                        }
                    }
                }
            }
            if (sMsg != null)
            {
                //string buildCsServoMask = Resources.BuildCsServoMask;
                //MessageBox1.Show(this, sMsg, buildCsServoMask, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //this.WriteErrorLog("NetLa71Main", "BuildCsServoMasks", flag ? ErrorLogTypes.Controller : ErrorLogTypes.Database, buildCsServoMask, sMsg);
            }
        }

        public void ProcessConnection(bool bConnected)
        {
            if (bConnected)
            {
                toolStripStatusLabel1.Text = "Cart Connected";
                toolStripStatusLabel1.ForeColor = Color.Green;
            }
            else
            {
                toolStripStatusLabel1.Text = "Cart Not Connected";
                toolStripStatusLabel1.ForeColor = Color.Red;
            }
            
        }

        private void StartConnectionTimer(bool bPresent)
        {
            if (bPresent)
            {
                this.tmrConnection.Interval = 0x3e8;
                this.tmrConnection.Start();
            }
        }
        private void tmrConnection_Tick(object sender, EventArgs e)
        {
            bool flag = this.CartBase.DeviceIsPresent();
            if (flag != this.bDevicePresent)
            {
                this.bDevicePresent = flag;
                Application.DoEvents();
                if (this.bDevicePresent)
                {
                    try
                    {
                        this.SetServoPulseWidths();
                        this.SetMotorRunTimes();
                        this.CartBase.RefreshDeviceParams();
                        //if (this.bUserLoggedIn)
                        //{
                        this.tmrConnection.Stop();
                        this.CartBase.SetStatusPolling(true);
                        //}
                    }
                    catch (La7UsbException exception)
                    {
                        //string sMsg = string.Format(Resources.ErrorRestartingTheCartInterfaceNX0, exception.Message);
                        //string reconnectToCart = Resources.ReconnectToCart;
                        //MessageBox1.Show(this, sMsg, reconnectToCart, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        //this.WriteErrorLog("NetLa71Main", "tmrConnection_Tick", ErrorLogTypes.Controller, reconnectToCart, sMsg);
                    }
                }
                this.ProcessConnection(this.bDevicePresent);
            }
        }

        public void SetServoPulseWidths()
        {
            double num;
            double num2;
            double num3;
            double num4;
            double num5;
            double num6;
            double.TryParse(LocalSettings.GetStringLocalSetting(LocalSettings.STR_LS_MainUnlockTime).Replace(".", "").Replace(",", ""), out num);
            num /= 1000.0;
            double.TryParse(LocalSettings.GetStringLocalSetting(LocalSettings.STR_LS_MainLockTime).Replace(".", "").Replace(",", ""), out num2);
            num2 /= 1000.0;
            double.TryParse(LocalSettings.GetStringLocalSetting(LocalSettings.STR_LS_DwrUnlockTime).Replace(".", "").Replace(",", ""), out num3);
            num3 /= 1000.0;
            double.TryParse(LocalSettings.GetStringLocalSetting(LocalSettings.STR_LS_DwrLockTime).Replace(".", "").Replace(",", ""), out num4);
            num4 /= 1000.0;
            double.TryParse(LocalSettings.GetStringLocalSetting(LocalSettings.STR_LS_CsUnlockTime).Replace(".", "").Replace(",", ""), out num5);
            num5 /= 1000.0;
            double.TryParse(LocalSettings.GetStringLocalSetting(LocalSettings.STR_LS_CsLockTime).Replace(".", "").Replace(",", ""), out num6);
            num6 /= 1000.0;
            try
            {
                this.CartBase.SetServoPulseTimes(num, num2, num3, num4, num5, num6);
            }
            catch (La7UsbException exception)
            {
                //string sMsg = string.Format(Resources.ErrorSettingServoPulseWidthsNX0, exception.Message);
                //string setServoPulseWidths = Resources.SetServoPulseWidths;
                //MessageBox1.Show(this, sMsg, setServoPulseWidths, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //this.WriteErrorLog("NetLa71Main", "SetServoPulseWidths", ErrorLogTypes.Controller, setServoPulseWidths, sMsg);
            }
        }

        public void SetMotorRunTimes()
        {
            int intLocalSetting = LocalSettings.GetIntLocalSetting(LocalSettings.STR_LS_MainRunTime);
            int drawerServos = LocalSettings.GetIntLocalSetting(LocalSettings.STR_LS_DrawerRunTime);
            int csServos = LocalSettings.GetIntLocalSetting(LocalSettings.STR_LS_CsServoRunTime);
            try
            {
                this.CartBase.SetMotorRunTimes(intLocalSetting, drawerServos, csServos);
            }
            catch (La7UsbException exception)
            {
                //string sMsg = string.Format(Resources.ErrorSettingServoMotorRunTimesNX0, exception.Message);
                //string setMotorRunTimes = Resources.SetMotorRunTimes;
                //MessageBox1.Show(this, sMsg, setMotorRunTimes, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //this.WriteErrorLog("NetLa71Main", "SetMotorRunTimes", ErrorLogTypes.Controller, setMotorRunTimes, sMsg);
            }
        }

        private void btnCycleDwrs_Click(object sender, EventArgs e)
        {
            int drawerMask = this.GetSelectedServos() & ~this.CsLockServoMask;
            try
            {
                this.CartBase.CycleDrawers(drawerMask);
            }
            catch (La7UsbException exception)
            {
                //string sMsg = string.Format(Resources.ErrorCyclingSelectedServosNX0, exception.Message);
                //string cycleSelected = Resources.CycleSelected;
                //MessageBox1.Show(this, sMsg, cycleSelected, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //this.Main.WriteErrorLog("CartOperationDlg", "btnCycleDwrs_Click", ErrorLogTypes.Controller, cycleSelected, sMsg);
                MessageBox.Show(exception.Message);
            }
            //AuditLogging.LogDrawersEvent(ActivityCodes.LOG_SVC_CYCLE_LA71_DWRS, drawerMask);
        }

        private void btnLockDwrs_Click(object sender, EventArgs e)
        {
            int selectedServos = this.GetSelectedServos();
            int drawerMask = selectedServos & this.CsLockServoMask;
            selectedServos &= ~drawerMask;
            try
            {
                if (selectedServos != 0)
                {
                    this.CartBase.LockDrawers(selectedServos);
                }
                if (drawerMask != 0)
                {
                    this.CartBase.LockCsServos(drawerMask);
                }
            }
            catch (La7UsbException exception)
            {
                //string sMsg = string.Format(Resources.ErrorLockingSelectedServosNX0, exception.Message);
                //string lockSelected = Resources.LockSelected;
                //MessageBox1.Show(this, sMsg, lockSelected, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //this.Main.WriteErrorLog("CartOperationDlg", "btnLockDwrs_Click", ErrorLogTypes.Controller, lockSelected, sMsg);
                MessageBox.Show(exception.Message);
            }
            //AuditLogging.LogDrawersEvent(ActivityCodes.LOG_SVC_LOCK_LA71_DWRS, (short)selectedServos);
        }

        private void btnLockAll_Click(object sender, EventArgs e)
        {
            try
            {
                this.CartBase.LockAllServos();
            }
            catch (La7UsbException exception)
            {
                //string sMsg = string.Format(Resources.ErrorLockingAllServosNX0, exception.Message);
                //string lockAllServos = Resources.LockAllServos;
                //MessageBox1.Show(this, sMsg, lockAllServos, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //this.Main.WriteErrorLog("CartOperationDlg", "btnLockAll_Click", ErrorLogTypes.Controller, lockAllServos, sMsg);
                MessageBox.Show(exception.Message);
            }
        }

        public void ProcessDeviceStatus(CLa7DevStatus Status)
        {
            int num = this.CurrentUnlocked ^ Status.Unlocked;
            int num2 = this.CurrentSwitches ^ Status.Switches;
            //this.picServosActive.Image = (((Status.Flags & 1) != 0) ? Resources.RedLedOn : Resources.RedLedOff);
            //this.picMotionDetect.Image = (((Status.Flags & 2) != 0) ? Resources.RedLedOn : Resources.RedLedOff);
            if ((num2 & 8) != 0)
            {
               // this.picMainLock.Image = (((Status.Switches & 8) != 0) ? Resources.RedLedOn : Resources.RedLedOff);
            }
            if ((num2 & 4) != 0)
            {
                //this.picMainLimit.Image = (((Status.Switches & 4) != 0) ? Resources.RedLedOn : Resources.RedLedOff);
            }
            for (int i = 0; i < this.UnlockedLeds.Length; i++)
            {
                
                if (((long)num & (long)((ulong)this.DrawerMasks[i])) != 0L)
                {
                    this.UnlockedLeds[i].Checked = (((Status.Unlocked & this.DrawerMasks[i]) != 0) ? true : false);
                    //this.UnlockedLeds[i].Image = (((Status.Unlocked & this.DrawerMasks[i]) != 0) ? Resources.RedLedOn : Resources.RedLedOff);
                }
                if (((long)num2 & (long)((ulong)this.DrawerMasks[i])) != 0L)
                {
                    this.OpenedLeds[i].Checked = (((Status.Switches & this.DrawerMasks[i]) != 0) ? true : false);
                    //this.OpenedLeds[i].Image = (((Status.Switches & this.Main.DrawerMasks[i]) != 0) ? Resources.RedLedOn : Resources.RedLedOff);
                }
            }
            this.CurrentSwitches = Status.Switches;
            this.CurrentUnlocked = Status.Unlocked;
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x112)
            {
                if (m.WParam == ((IntPtr)0x460))
                {
                    //this.CartBase.DemoFlag = true;
                    //this.bUseDemoPatients = true;
                    return;
                }
                if (m.WParam == ((IntPtr)0x420))
                {
                    try
                    {
                        this.CartBase.CycleUsbPort();
                    }
                    catch (La7UsbException exception)
                    {
                        //string sMsg = string.Format(Resources.CantCycleTheUsbPortNX0, exception.Message);
                        //string cycleUsbPort = Resources.CycleUsbPort;
                        //MessageBox1.Show(this, sMsg, cycleUsbPort, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        //this.WriteErrorLog("NetLa71Main", "WndProc", ErrorLogTypes.Miscellaneous, cycleUsbPort, sMsg);
                    }
                    return;
                }
            }
            base.WndProc(ref m);
        }

        private void NetLa71Main_Shown(object sender, EventArgs e)
        {
            
            
        }
    }
}
