using System; using System.Collections.Generic; using System.Threading; using System.Threading.Tasks; using System.Timers; using Foundation; using ServiceTool.Engineering;
using ServiceTool.Legacy.Dongle; using ServiceTool.Legacy.REM; using ServiceTool.Network;  using UIKit;   
namespace Controller_SRDO
{
    public partial class ViewController : UIViewController
    {
        PortSwitch PortSwitch;          public List<Srdo> Page;
                private Reachability hostReachability;
                private bool zkipStatus;        string Rows;         TypePickerModel typepickerModel;

        public ViewController(IntPtr handle) : base(handle)
        {
           

        }

        public ViewController()
        {
        }

        List<string> Type = new List<string> {             "SRDO",             "RTD",             "CR",          };
        public override async void ViewDidLoad()         {             base.ViewDidLoad();             SetupPicker();
            ResponseText.Text = strSRDOResp;


            // Perform any additional setup after loading the view, typically from a nib.
            //Dongle
            var ipAddress = ServiceTool.Network.Helper.GetIPAddress();             if (ipAddress.Contains("10.10.10"))             {                 PortSwitch = new PortSwitch();                 await PortSwitch.ConnectDongle();                 PortSwitch.OnPortStatus += (sender, e) => PortStatus(e);                 PortSwitch.OnPortswapError += (sender, e) => PortSwitchStatus(e);                  PortSwitch.SrdoLocal.OnReadRsp += (sender, e) => Process(e);                 PortSwitch.SrdoLocal.OnWriteRsp += (sender, e) =>  Process(e);                 PortSwitch.SrdoLocal.OnReadParaRsp += (sender, e) =>  Process(e);                 PortSwitch.SrdoLocal.OnWriteParaRsp += (sender, e) =>  Process(e);                 PortSwitch.SrdoLocal.OnCommandRsp += (sender, e) =>  Process(e);                 PortSwitch.SrdoLocal.OnRTDRsp += (sender, e) =>  Process(e);                 PortSwitch.SrdoLocal.OnRTDStopRsp += (sender, e) =>  Process(e);                  PortSwitch.OnDongleDisconnet += (sender, e) => Disconnect();                 PortSwitch.OnPortZkip += (sender, e) => ZkipStatus(e);             }             else             {                 var okAlertController = UIAlertController.Create("Connection Issue", "Please connect SVT dongle...", UIAlertControllerStyle.Alert);                 okAlertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));                 PresentViewController(okAlertController, true, null);             }              //SaveBtn.TouchUpInside += SaveResponse;             this.NavigationItem.SetRightBarButtonItem(             new UIBarButtonItem(                     UIBarButtonSystemItem.Done,                     SendMessageAsync)             , true);               // Change the host name here to change the server you want to monitor.             //const string remoteHostName = "10.10.10.1";             System.Net.IPAddress ipAdd = System.Net.IPAddress.Parse("10.10.10.1");                         hostReachability = Reachability.ReachabilityWithAddress(ipAdd);             hostReachability.StartNotifier();         }

        private void SetupPicker()
        {             typepickerModel = new TypePickerModel(Type);             typepickerModel.ValueChanged += (sender, e)=>
            {
                if (typepickerModel.SelectedValue!= null )
                {
                   Rows = typepickerModel.SelectedValue;
                    //SRDOText.Text = typepickerModel.SelectedValue;
                }
               // else ondesignationselection = "";
            };
            PickerView.Model = typepickerModel;
          
          

        }

        void Disconnect()         {             Console.WriteLine("Dongle connected closed");         }          public override void ViewDidAppear(bool animated)         {             base.ViewDidAppear(animated);              // Observe the kNetworkReachabilityChangedNotification. When that notification is posted, the method reachabilityChanged will be called.             NSNotificationCenter.DefaultCenter.AddObserver(new NSString(Reachability.ReachabilityChangedNotification), OnReachabilityChanged);         }          public override void ViewDidDisappear(bool animated)         {             base.ViewDidDisappear(animated);                     }          private void OnReachabilityChanged(NSNotification notification)         {             var reachability = notification.Object as Reachability;             UpdateInterfaceWithReachability(reachability);         }          private void UpdateInterfaceWithReachability(Reachability reachability)         {             if (reachability == hostReachability)             {                 var networkStatus = reachability.CurrentReachabilityStatus();                 var connectionRequired = reachability.ConnectionRequired();                  Console.WriteLine(networkStatus.ToString());                  if (networkStatus == NetworkStatus.NotReachable)                 {                     var okAlertController = UIAlertController.Create("Connection Issue", "No Wi-Fi", UIAlertControllerStyle.Alert);                     okAlertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));                     PresentViewController(okAlertController, true, null);                 }                 else                 {                     var okAlertController = UIAlertController.Create("Connection Issue", "Wi-Fi Connected", UIAlertControllerStyle.Alert);                     okAlertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, async (action) => await ConnectDongle()));                     PresentViewController(okAlertController, true, null);                 }             }         }          protected override void Dispose(bool disposing)
        {             base.Dispose(disposing);              NSNotificationCenter.DefaultCenter.RemoveObserver(this, Reachability.ReachabilityChangedNotification); 
        }          async Task ConnectDongle()         {             var ipAddress = ServiceTool.Network.Helper.GetIPAddress();             if (ipAddress.Contains("10.10.10"))             {                 PortSwitch = new PortSwitch();                 await PortSwitch.ConnectDongle();                 PortSwitch.OnPortStatus += (sender, e) => PortStatus(e);                 PortSwitch.OnPortswapError += (sender, e) => PortSwitchStatus(e);                  PortSwitch.SrdoLocal.OnReadRsp += (sender, e) => Process(e);                 PortSwitch.SrdoLocal.OnWriteRsp += (sender, e) => Process(e);                 PortSwitch.SrdoLocal.OnReadParaRsp += (sender, e) => Process(e);                 PortSwitch.SrdoLocal.OnWriteParaRsp += (sender, e) => Process(e);                 PortSwitch.SrdoLocal.OnCommandRsp += (sender, e) => Process(e);                 PortSwitch.SrdoLocal.OnRTDRsp += (sender, e) => Process(e);                 PortSwitch.SrdoLocal.OnRTDStopRsp += (sender, e) => Process(e);                  PortSwitch.OnDongleDisconnet += (sender, e) => Disconnect();             }         }          public override void DidReceiveMemoryWarning()         {             base.DidReceiveMemoryWarning();             // Release any cached data, images, etc that aren't in use.         }          public async void SendMessageAsync(object sender, EventArgs e)         {             if (PortSwitch != null)             {                 //StatusTextView.Text = " ";                  await ReadSrdo();                 await WriteSrdo();                 await Task.Delay(200);                 await Command();                 //await Task.Delay(200);                 //await RTD();                 await Task.Delay(200);                 await WriteSrdoPara();                 await Task.Delay(200);                 await ReadSrdoPara();                 await Task.Delay(200);                 //await PortSwitch.SwapBackToSVT();                 await ReadAllSRDOS();                 await PortSwitch.Resend();             }             else             {                 var okAlertController = UIAlertController.Create("Error", "Not Connected!", UIAlertControllerStyle.Alert);                 okAlertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));                 PresentViewController(okAlertController, true, null);             }         }

        #region UI         //void UpdateTextView(string myUpdate)
                           //{
                           //    StatusTextView.Text = StatusTextView.Text + "\n" + myUpdate;
                           //}
        #endregion //UI 
        #region SRDO_TEST         public async Task ReadSrdo()         {
            // UpdateTextView("Sending SRDO READ: 0 " );
            uint[] rdo = { 254 };
           // await GetScene(rdo);

        
            int input = Convert.ToInt32(SRDOText.Text);             uint x= Convert.ToUInt32(input);             rdo[0] = (uint)input;
            //foreach (uint i in rdo)
           
               await PortSwitch.SrdoLocal.SendMessageRead(0, rdo, 0x01);

            //}

        }
          public async Task WriteSrdo()         {             SRDOWrite srdo1 = new SRDOWrite();             SRDOWrite srdo2 = new SRDOWrite();             List<SRDOWrite> srdoList = new List<SRDOWrite>();                           srdo1.SRDO = 101;             srdo1.Length = 1;             byte[] wData = { 2 };             srdo1.Data = wData;              srdo2.SRDO = 102;             srdo2.Length = 1;             byte[] wDataA = { 3 };             srdo2.Data = wDataA;              srdoList.Add(srdo1);             srdoList.Add(srdo2);             // UpdateTextView("Sending SRDO WRITE: #101 D:2 #102 D:3 " );              await PortSwitch.SrdoLocal.SendMessageWrite(0, srdoList.ToArray(), 0x01);              //UpdateTextView("Sending SRDO READ: #101, #102");             uint[] rdoW = { 101, 102 };             await PortSwitch.SrdoLocal.SendMessageRead(0, rdoW, 0x01);         }          public async Task Command()         {             //byte[] cR = {0x00, 0x00, 0x01, 0x01};             byte[] cR = { 0x15, 0x00 };             SRDOCommand srdoC = new SRDOCommand()             {                 Command = 2,                 ArgumentLength = 2,                 Argument = cR             };              //UpdateTextView("Sending COMMAND DIALOG: Command->2 Argument-> 0, 0, 1, 1" );             await PortSwitch.SrdoLocal.SendMessageCommandDialog(srdoC, 0x01);         }          public async Task RTD()         {             uint[] srdo = { 0, 1, 2, 3, 4, 5, 6 };              RTDStart rtd = new RTDStart()             {                 Type = 5,                 PollRate = 0,                 Page = 2,                 Srdo = srdo             };              //UpdateTextView("Sending Start RTD for SRDO 0,1,2,3,4,5,6 ");             await PortSwitch.SrdoLocal.SendMessageRTDStart(rtd, 0x01);               await RTDStopTimerAsync();          }          public async Task ReadSrdoPara()         {             SRDOReadParameterized srdoPara1 = new SRDOReadParameterized             {                 SRDO = 1117,                 P1 = 0,                 P2 = 2,                 P3 = 0             };              //UpdateTextView("Sending READ SRDO Parameterized: " + srdoPara1.SRDO +                 //" P1 " + srdoPara1.P1 +                 //" P2 " + srdoPara1.P2 +                 //" P3 " + srdoPara1.P3);               SRDOReadParameterized srdoPara4 = new SRDOReadParameterized             {                 SRDO = 1117,                 P1 = 0,                 P2 = 4,                 P3 = 0             };              //UpdateTextView("Sending READ SRDO Parameterized: " + srdoPara4.SRDO +                 //" P1 " + srdoPara4.P1 +                 //" P2 " + srdoPara4.P2 +                 //" P3 " + srdoPara4.P3);              SRDOReadParameterized[] srdoList = { srdoPara1, srdoPara4 };             await PortSwitch.SrdoLocal.SendMessageRead(0, srdoList, 0x01);         }          public async Task WriteSrdoPara()         {             byte[] writePara = { 0x07 };             byte[] writePara1 = { 0x05, 0x09 };               SRDOWriteParameterized srdoPara1 = new SRDOWriteParameterized             {                 SRDO = 1117,                 P1 = 0,                 P2 = 2,                 P3 = 0,                 Length = 1,                 Data = writePara             };             //UpdateTextView("Sending WRITE SRDO Parameterized: " + srdoPara1.SRDO +                 //" P1 " + srdoPara1.P1 +                 //" P2 " + srdoPara1.P2 +                 //" P3 " + srdoPara1.P3 +                 //" Data " + ByteArrayToString(srdoPara1.Data));              SRDOWriteParameterized srdoPara2 = new SRDOWriteParameterized             {                 SRDO = 1117,                 P1 = 0,                 P2 = 4,                 P3 = 0,                 Length = 2,                 Data = writePara1             };              //UpdateTextView("Sending WRITE SRDO Parameterized: " + srdoPara2.SRDO +                 //" P1 " + srdoPara2.P1 +                 //" P2 " + srdoPara2.P2 +                 //" P3 " + srdoPara2.P3 +                 //" Data " + ByteArrayToString(srdoPara2.Data));              SRDOWriteParameterized[] srdoList = { srdoPara1, srdoPara2 };             await PortSwitch.SrdoLocal.SendMessageWrite(0, srdoList, 0x01);         }          public async Task RTDStopTimerAsync()         {              await Task.Delay(5000);            //UpdateTextView("Sending Stop RTD for 0,1,2,3,4,5,6 ");             await PortSwitch.SrdoLocal.SendMessageRTDStop(0x01);         }          public override void TouchesBegan(NSSet touches, UIEvent evt)
        {             SRDOText.ResignFirstResponder();
            base.TouchesBegan(touches, evt);
        }

      
        async partial void SendButton_TouchUpInside(UIButton sender)         {             try
            {                 ////UIStoryboard board = UIStoryboard.FromName("Main", null);                 ////ResponseViewController ctrl = (ResponseViewController)board.InstantiateViewController("ResponseViewController") as ResponseViewController;                 //ctrl.MyProperty = strSRDOResp;                 //this.NavigationController.PushViewController(ctrl, true);

                if (Rows == "SRDO")                 { 
                    await ReadSrdo();

                }
                else if (Rows == "RTD")
                {
                    await RTD();
                }
                else
                {
                    await Command();
                }

            }             catch (Exception ex)
            {
                Console.WriteLine("" + ex);
            }
        }

      
   

        public async Task ReadAllSRDOS()         {             string FilePath = "Engineering/Helper/Test/GlobalOp_1.json";             ParseSRDO helper = ParseSRDO.SharedInstance();             helper.ParseFileAtPath(FilePath);              Page = helper.rdoModelObj.page[0].srdo;              foreach (Srdo item in Page)             {                 uint[] rdo = { (uint)item.rdo };                 await Task.Delay(150);                 await PortSwitch.SrdoLocal.SendMessageRead(0, rdo, 0x01);             }          }          #endregion //SRDO_TEST          #region EVENT_HANDLERS         public void PortStatus(string s)         {            //StatusLabel.Text = s;         }

                public void PortSwitchStatus(int status)         {             string alertMsg = "No Error";             string alertTitle = "Error";              switch (status)             {                 case ServiceTool.Legacy.Dongle.Constants.SVTLogonError:                     alertTitle = "Error";                     alertMsg = "REM protocol not supported";                     break;                 case ServiceTool.Legacy.Dongle.Constants.SVTConnected:                     alertTitle = "Success";                     alertMsg = "REM protocol is supported";                     break;                 case ServiceTool.Legacy.Dongle.Constants.SVTNoResponse:                     alertTitle = "Error";                     alertMsg = "No Response from GECB...";                     break;             }              var okAlertController = UIAlertController.Create(alertTitle, alertMsg, UIAlertControllerStyle.Alert);             okAlertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));             PresentViewController(okAlertController, true, null);         }           public void ZkipStatus(bool status)         {             if(status == true)             {                 zkipStatus = true;             }             else             {                 zkipStatus = false;             }          }          public static string ByteArrayToString(byte[] ba)         {             return BitConverter.ToString(ba).Replace("-", " ");         }          public string strSRDOResp = " " ;// TODO: Delete if not works          public void Process(SRDOReadResponse read)         {           string srdoRead;             if ( read.SRDO == 1 )             {                 srdoRead = "Rcv' Read SRDO: " + read.SRDO + " Data: " + System.Text.Encoding.UTF8.GetString(read.Data, 0, read.Data.Length);                 Console.WriteLine("srdoRead :" + srdoRead);                 strSRDOResp = srdoRead;             }             else             {                                   if (read.Error == 0)                 {                     srdoRead = "Rcv' Read SRDO: " + read.SRDO + " Size: " + read.Length + " Data: " + ByteArrayToString(read.Data);                 }                 else                  {                     srdoRead = "Rcv' Read SRDO: " + read.SRDO + " Error: " + read.Error;                 }              }              //UpdateTextView(srdoRead);             Console.WriteLine("ProcessRead");         }          public void Process(SRDOWriteResponse write)         {             string srdoWrite;             if ( write.Length == 0)             {                 srdoWrite = "Rcv' Write SRDO: " + write.SRDO + " Success! " ;             }             else             {                 srdoWrite = "Rcv' Write SRDO: " + write.SRDO + " Failed! " ;             }              //UpdateTextView(srdoWrite);             Console.WriteLine("SRDOWriteResponse");         }          public void Process(SRDOReadParameterizedResponse srdo)         {             string msg;             if (srdo.Error == 0)             {                 msg = "Rcv' Read Para SRDO: " + srdo.SRDO + " Data: " + ByteArrayToString(srdo.Data) + " Success! ";             }             else             {                 msg = "Rcv' Read Para SRDO: " + srdo.SRDO + " Failed! ";             }             //UpdateTextView(msg);         }          public void Process(SRDOWriteParameterizedResponse srdo)         {             string msg;             if ( srdo.Length == 0)             {                 msg = "Rcv' Write Para SRDO: " + srdo.SRDO + " Success! " ;             }             else             {                 msg = "Rcv' Write Para SRDO: " + srdo.SRDO + " Failed! " ;             }             //UpdateTextView(msg);         }          public void Process(SRDOCommandResponse srdo)         {             string msg;              msg = "Rcv' Command Response: " + srdo.Response + " Argument Length " + srdo.ArgumentLength;             // UpdateTextView(msg);         }          public void Process(RTDDataResponse rtd)         {             string msg;              if (rtd.Length > 0)             {                 msg = "Rcv' RTD Data: " + ByteArrayToString(rtd.Data);                 //UpdateTextView(msg);             }         }                  public void Process(RTDStopResponse rtd)         {             string msg;             msg = "Rcv' RTD STOP ";            // UpdateTextView(msg);         }          #endregion //EVENT_HANDLERS     } } 

        //partial void SendButton_TouchUpInside(UIButton sender)
        //{
            
           

        //    UIStoryboard Storyboard = UIStoryboard.FromName("Main", null);

        //    var contentController = (ConnectionViewController)Storyboard.InstantiateViewController("ConnectionViewController");

        //    this.NavigationController.PushViewController(contentController, true);

        //}


      
             



