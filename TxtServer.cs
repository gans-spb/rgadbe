/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Ioffe inst., Igor Bocharov
 * Simple text Telnet IP async server
 */

using System.Threading;
using System.Net;
using System.Net.Sockets;

class TxtServer
{
    IPEndPoint ipep;
    public bool isRun;

    public TxtServer(string host, int port){
        IPAddress ip = IPAddress.Parse(host);
        ipep = new(ip, port);
        isRun = false;
        cList = [];
    }

    //blocked stop
    public void Stop(){
        cts.Cancel();
        isRun = false;
    }

    //send message for all clients
    public void SendAll(string msg){
        foreach(var cl in cList)
            cl.SendAsync(msg);
    }

    //String with client ip list
    List<TxtClient> cList;
    public StrEvtCb sendClientListCb;

    void SendClientList()
    {
        if (cList.Count==0)
            if (isRun) sendClientListCb("nobody");
                  else sendClientListCb("closed");
        else{
            string cinfo = "";
            foreach(var cl in cList)
                cinfo += (cl.addrStr + "\n");
            cinfo = cinfo.TrimEnd('\n');
            sendClientListCb(cinfo);
        }
    }

    //get message from a client
    //see on01sTick TelnetDeque
    public Queue gotAllMsg = new();

    void GotClientCmd(string cmd){
        lock(gotAllMsg.SyncRoot)
            gotAllMsg.Enqueue(cmd);
    }
    
    TcpListener listn;
    private CancellationTokenSource cts;

    public async Task Run()
    {
        try{
            listn = new(ipep);
            listn.Start();
            isRun = true;
            Log.Information("Start ip server " + ipep.Port + " port");

            cts = new CancellationTokenSource();

            while (isRun)
            {
                await Task.Delay(100);
                var ipcl = await listn.AcceptTcpClientAsync(cts.Token); //wait here

                //new clt
                var clnt = new TxtClient(ipcl, GotClientCmd);
                Log.Information("IP client " + clnt.addrStr + " conn");
                cList.Add(clnt);
                SendClientList();
                var clientTask = clnt.Run(); //don't await
                
                //disconnected clt
                clientTask.ContinueWith(t => {
                    Log.Information("IP client " + clnt.addrStr + " disconn");
                    cList.Remove(clnt);
                    SendClientList();
                });
            }

        }catch(Exception e){
            if (e is OperationCanceledException) 
                Log.Information("Stop ip server"); //normal way to stop listener
            else 
                Log.Error("Error ip server: " + App.StringFromEx(e));
            }
        finally{
            foreach(var cl in cList) cl.Stop();
            cList.Clear();
            listn.Stop();
            SendClientList();
        }
    }
}


class TxtClient
{
    TcpClient clnt;
    NetworkStream stream;
    public bool isRun = false;

    //at start cause on dispored on client disconnect
    public string addrStr;
    public void GetAddrStr(){
        IPEndPoint lipep = clnt.Client.LocalEndPoint  as IPEndPoint;
        IPEndPoint ripep = clnt.Client.RemoteEndPoint as IPEndPoint;
        addrStr = ripep.Address.ToString() + ":" + ripep.Port.ToString();
    }

    StrEvtCb gotMsg;
    public TxtClient(TcpClient client, StrEvtCb _gotMsg)
    {
        gotMsg = _gotMsg;
        this.clnt = client;
        GetAddrStr();
    }

    System.IO.StreamReader sr;
    System.IO.StreamWriter sw;

    public async Task Run()
    {
        stream = clnt.GetStream();   
        sr = new System.IO.StreamReader(stream);
        sw = new System.IO.StreamWriter(stream);

        await SendAsync("Extorr RGA DB " + AppConst.Ver + " hello!"+nl
            + "try 'ccmd:help' as start point." + nl) ;
        isRun = true;

        while (isRun){          
            var msg = await sr.ReadLineAsync();    //wait here
            Thread.Sleep(50); 
            //TTY negotiation remover
            msg = System.Text.RegularExpressions.Regex.Replace(msg,  @"[^\t\r\n -~]", string.Empty);
            msg = msg.Trim();
            if (msg!=null) 
                if (msg!="exit") gotMsg(msg);
                else Stop();
            else Stop();    //!!! stupidess method if client disconnected
        }
    }

    const string nl = "\r\n";//VT100 CR octal 015 code + \n

    public async Task SendAsync(string msg){
        await sw.WriteAsync(msg);
        await sw.FlushAsync();
    }
    
    public void Stop(){
        isRun = false;
        clnt.GetStream().Close();
        clnt.Close();
    }
}

//remove Putty crap at first: ff fb 1f ff fb 20 ff fb 18 ff fb 27 ff fd 01 ff fb 03 ff fd 03
//google "PuTTY sending extra characters at the beginning of TELNET connection initialisation"
//Putty->Connection->Telnet->Telnet negotiation mode = Passive
