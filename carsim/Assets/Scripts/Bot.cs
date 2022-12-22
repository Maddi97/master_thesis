using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
public class Bot : MonoBehaviour
{
	[SerializeField] public int count;
	public GameObject prefab;
	public float speed;//Speed Multiplier
    public float rotation;//Rotation multiplier
	public int millipusish = 1000;
	public int resWidth;
	public int resHeight;
	private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";
	
    [SerializeField] private float motorForce;
    [SerializeField] private float maxSteerAngle;
    
    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheeTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;
    
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;
	//public Camera camera;
	private long starttime;
	private bool humanpilot = false;
	private float acc = 0;
	private float steer = 0;
	private string adress;
	private double score = 0;
	private int port;
	private bool newpic;
	private bool humanstart = false;
	private byte[] picture;
	private long duration;
	private float dist;
	private float currentSteerAngle;
	private Thread receiveThread; //1
	private Thread sendThread;
	private UdpClient client; //2
	private UdpClient server; 
	private bool gamegoson;
	private bool recvstop = true;
	private bool sendstop = true;
	public Camera camera;
    void FixedUpdate()//FixedUpdate is called at a constant interval
    {
		if (!humanstart){
			if(Input.GetKey(KeyCode.Space)){
				humanstart = true;
				starttime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
				print("start");
			}
		}else{
			if(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()-starttime>= 40000){
				dist = 72.8f-(-26)-(72.8f-transform.position.z);
	//			print(dist);
				long time = (new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()+(count*millipusish))-starttime;
				score = dist/(time);
//				print(score);
			}
		}
		
		
		if (humanpilot){
			GetInput();
		}
		
		HandleMotor();
        HandleSteering();
        UpdateWheels();
       
	}
    
    // Start is called before the first frame update
    void Start()
    {
		if (!humanpilot)
		{
			starttime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
			InitUDP();
		}
		gamegoson = true;
		setupcam();

    }

    // Update is called once per frame
    void Update()
    {
		if (gamegoson && camera != null && !humanpilot){
			picture = CamCapture();
			if (picture.Length>0){
				newpic = true;
			}
			
		}
        
    }
    
    private void GetInput()
    {
		steer = Input.GetAxis(HORIZONTAL);
        acc  = Input.GetAxis(VERTICAL);
    }

    
    public void sethumandriver(bool human){
		humanpilot = human;
	}
    
    public void setportandaddress(int aport, string aadress){
		port = aport;
		adress = aadress;
	}
	
	private void setupcam(){
		GameObject temp  = Instantiate(prefab,transform.position+ new Vector3(0,5,0),new Quaternion(0, 0, 0, 0));
		CameraFollow script = temp.GetComponent<CameraFollow>();
		script.settransform(transform);
		camera = temp.GetComponent<Camera>();
	}
	private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = acc * motorForce;
        frontRightWheelCollider.motorTorque = acc * motorForce;    
    }
	
	private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * steer;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }
	
	  private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheeTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot
;       wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }
    
    
	
	public double getscore(){
		gamegoson = false;
		//Destroy(camera);
		this.receiveThread.Interrupt();
		this.sendThread.Interrupt();
		this.sendThread.Join();
		this.receiveThread.Join();
		this.server.Close();
		this.client.Close();
		this.client.Dispose();
		this.server.Dispose();
		this.recvstop = false;
		this.sendstop = false;
		if (score == 0){
			print("calcu");
			dist = 72.8f-(-26)-(72.8f-transform.position.z);
			print(dist);
			long time = (new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()+(count*millipusish))-starttime;
			//print(time);
			score = dist/(time);
			//print(score);
		}
		try{
			GameObject.Destroy(camera.gameObject);
		}catch(Exception e){
			print (e.ToString());
		}
		camera = null;
		return score;
	}
	
	public bool getnothread(){
		
		return (!recvstop && !sendstop);
	}
    
    public long Getduration(){
		return duration;
	}

    private void OnTriggerEnter(Collider other)
    {
	    if (other.tag == "red" && !(isleft(other.transform.position))){
			Debug.Log("red");
		   count++;
		}else if(other.tag == "blue" && isleft(other.transform.position)){
			Debug.Log("blue");
			count++;
		}else if (other.tag == "finish"){
			duration =  (new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()+count*millipusish)-starttime;
			dist = 72.8f-(-26)-(72.8f-transform.position.z);// Vector3.Distance(new Vector3(0,-1.7f,-26), );
			score = dist/duration;
			print(score);
			gamegoson = false;
		}else{
			//Debug.Log("Tag Problem:"+other.tag);
		}	    
    }
    
    private bool isleft(Vector3 position){
		Vector3 Dir = position - transform.position;
		Dir = Quaternion.Inverse(transform.rotation) * Dir;
		return (Dir.x>0);
	}
	
	private byte[] CamCapture()
    {
		try{
			RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
			camera.targetTexture = rt;
			Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
			camera.Render();
			RenderTexture.active = rt;
			screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
			camera.targetTexture = null;
			RenderTexture.active = null; // JC: added to avoid errors
			Destroy(rt);
			return screenShot.EncodeToPNG();
		}catch(Exception e){
				print (e.ToString());
				return new byte[0];
		}
		
        
		
    }
    
    private void InitUDP(){
		print ("UDP Initialized");
		this.receiveThread = new Thread (new ThreadStart(UPDrecv));
		this.receiveThread.IsBackground = true;
		this.receiveThread.Start();
		this.sendThread = new Thread (new ThreadStart(UDPstuff));
		this.sendThread.IsBackground = true;
		this.sendThread.Start();
	}
	
	private void UPDrecv(){
		server = new UdpClient(port+2);
		IPEndPoint localEp = new IPEndPoint(IPAddress.Parse(adress), 0);
		//server.Client.Bind(localEp);
		byte[] data;
		while (true){
			try{
				if(!gamegoson){
					recvstop = false;
					server.Close();
					break;
				}
				data = server.Receive(ref localEp);
				string text = Encoding.UTF8.GetString(data); 
				if (text == "ENDREG"){
					recvstop = false;
					break;
				}
				string[] texts = text.Split(";");
				acc = float.Parse(texts[0]);
				steer = float.Parse(texts[1]);
			} 
			catch(Exception e){
				print (e.ToString());
			}
		}
		
	}
    
    
    
    
	
	private void UDPstuff(){
		client = new UdpClient(port);
		byte[] startcode = System.Text.Encoding.UTF8.GetBytes("LosGehtsKleinerHase");
		byte[] endcode = System.Text.Encoding.UTF8.GetBytes("ZuEndekleinerHase");
		byte[] eog = System.Text.Encoding.UTF8.GetBytes("ENDOFGAME");
		print("Port:"+ port.ToString());
		int port2 = port+3;
		IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse(adress),port2);
		while (true){
			try{
				if (!gamegoson){
					client.Send(eog, eog.Length,anyIP );
					client.Close();
					sendstop = false;
					break;
				}
				if (newpic){
					int len = picture.Length;
					//print (len.ToString());
					client.Send(startcode,startcode.Length, anyIP);
					while (true){
						if (picture.Length < 8654){
							client.Send(picture,picture.Length, anyIP);
							//print("sending the rest!");
							
							break;
						}
						byte[] first = new byte[8654];
						Buffer.BlockCopy(picture, 0, first, 0, first.Length);
						byte[] second = new byte[picture.Length - first.Length];
						Buffer.BlockCopy(picture, first.Length, second, 0, second.Length);
						client.Send(first,first.Length,anyIP);
						picture = second;
						//print(picture.Length.ToString());
						
					}
					client.Send(endcode,endcode.Length, anyIP);
					newpic = false; 
				}
			} 
			catch(Exception e){
				print (e.ToString()); 
			}
		}
		
	}
				
	
	
	
}
