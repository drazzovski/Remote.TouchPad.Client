using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class Main : MonoBehaviour
{
    private WebSocket ws;
    System.Diagnostics.Stopwatch stopWatch;

    public InputField portInput;
    public InputField hostInput;
    private GameObject empty1;
    public GameObject emptyExtraButtons;
    public Toggle doneAsEnter;
    TouchScreenKeyboard keyboard;

    private bool empty1MoveDown = false;
    private bool highlightingStarted = false;
    private bool startedVerticalScroll = false;
    private bool startedHorizontalScroll = false;
    private bool beganOne = false;
    private bool beganTwo = false;
    private bool zoomStarted = false;
    private bool cancelClick = false;

    private int touchCount = 0;
    private int sendX = 0;
    private int sendY = 0;

    List<string> buttons;
    // touch count 1
    private int oldPositionOneX = 0;
    private int oldPositionOneY = 0;
    private int startPositionOneX = 0;
    private int startPositionOneY = 0;

    // touch count 2
    private int oldPositionTwoX = 0;
    private int oldPositionTwoY = 0;
    private int startPositionTwoX = 0;
    private int startPositionTwoY = 0;

    // test
    public Text isConn;
    public Text typing;
    public Text status;

    int keyboardTextLength = 0;
    float emptyOneY;
    float emptyOneX;
    List<RaycastHit2D> list;

    private void Awake()
    {
        empty1 = GameObject.FindWithTag("Empty1");
        stopWatch = new System.Diagnostics.Stopwatch();
        buttons = new List<string>();
        emptyOneY = empty1.transform.position.y;
        emptyOneX = empty1.transform.position.x;
        list = new List<RaycastHit2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        emptyExtraButtons.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        if (empty1MoveDown && ws != null && ws.IsAlive)
        {
            empty1.transform.position += new Vector3(0, -1 * Time.deltaTime * 400, 0);
            if (empty1.transform.position.y < -260)
            {
                emptyExtraButtons.SetActive(true);
                empty1MoveDown = false;
            }
        }

        if (empty1.transform.position.y < -250 && ws != null && !ws.IsAlive)
        {
            empty1.transform.position = new Vector3(emptyOneX, emptyOneY, 0);
            emptyExtraButtons.SetActive(false);
        }

        touchCount = Input.touchCount;
        if (touchCount == 0)
        {
            highlightingStarted = false;
            startedVerticalScroll = false;
            startedHorizontalScroll = false;
            beganOne = false;
            beganTwo = false;
            zoomStarted = false;
            cancelClick = false;
        }
        
        if (keyboard != null && keyboard.status == TouchScreenKeyboard.Status.Visible)
        {
            if (keyboardTextLength < keyboard.text.Length)
            {
                keyboardTextLength = keyboard.text.Length;
                var character = keyboard.text[keyboard.text.Length - 1].ToString();
                ws.Send("key," + character);
                typing.text = keyboard.text[keyboard.text.Length - 1].ToString();
            }
            else if (keyboardTextLength > keyboard.text.Length)
            {
                typing.text = "backspace";
                ws.Send("key,backspace");
                keyboardTextLength--;
            }

        } else if (keyboard != null && keyboardTextLength > 0 && keyboard.status != TouchScreenKeyboard.Status.Visible)
        {
            keyboardTextLength = 0;
            if (keyboard.status == TouchScreenKeyboard.Status.Done && doneAsEnter.isOn)
            {
                ws.Send("key,enter");
            }
        }

        TouchLogic();
    }

    void OnApplicationQuit()
    {
        if (ws != null)
            ws.Close();
    }

    private void TouchLogic()
    {
        if (touchCount == 0)
            return;

        if (empty1MoveDown && ws != null && !ws.IsAlive)
            return;

        if (buttons.Count > 0)
        {
            ws.Send("buttons," + string.Join(",", buttons));
        }

        if (IsPointerOverUIObject(touchCount))
            return;

        //Debug.Log(touchCount == 1 || (touchCount - buttons.Count == 1));
        if ((touchCount == 1 && touchCount != buttons.Count) || (touchCount - buttons.Count == 1))
        {

            Touch touch = Input.GetTouch(touchCount - 1);
            
            var newPositionX = (int)touch.position.x;
            var newPositionY = (int)touch.position.y;
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    oldPositionOneX = newPositionX;
                    oldPositionOneY = newPositionY;
                    startPositionOneX = newPositionX;
                    startPositionOneY = newPositionY;
                    stopWatch.Start();
                    Debug.Log("BEGAN 1 --- " + stopWatch.IsRunning);
                    beganOne = true;
                    break;

                case TouchPhase.Moved:
                    if (!IsInRange((int)touch.position.x, startPositionOneX - 8, startPositionOneX + 8) && IsInRange((int)touch.position.y, startPositionOneY - 8, startPositionOneY + 8))
                        cancelClick = true;

                    if (oldPositionOneX < newPositionX)
                    {
                        sendX = 1;
                    }
                    else if (oldPositionOneX > newPositionX)
                    {
                        sendX = -1;
                    }
                    else
                    {
                        sendX = 0;
                    }

                    if (oldPositionOneY < newPositionY)
                    {
                        sendY = 1;
                    }
                    else if (oldPositionOneY > newPositionY)
                    {
                        sendY = -1;
                    }
                    else
                    {
                        sendY = 0;
                    }

                    if (sendX != 0 || sendY != 0)
                    {
                        var sensX = Mathf.Abs(oldPositionOneX - newPositionX);
                        var sensY = Mathf.Abs(oldPositionOneY - newPositionY);
                        if (sensX > 50)
                        {
                            sensX += (int)(90 * 1.2);
                            if (sensY < 15)
                            {
                                sensX += 50;
                            }
                        }
                        if (sensY >= 30)
                        {
                            sensY += (int)(90 * 1.2);
                            if (sensX < 15)
                            {
                                sensY += 20;
                            }
                        }

                        if (beganTwo && buttons.Count == 0)
                            return;

                        Debug.Log("SEND> X: " + (sendX * (sensX + sensY)) + " Y:" + (-sendY * (sensY + sensY)) + " btns:" + buttons.Count);

                        ws.Send($"{sendX * (sensX + sensY)},{-sendY * (sensY + sensY)}");

                    }
                    oldPositionOneX = newPositionX;
                    oldPositionOneY = newPositionY;
                    break;

                case TouchPhase.Ended:
                    if (beganTwo && !highlightingStarted)
                    {
                        SendSingleClick(touch, touch, "2a");
                        return;
                    }
                    if (highlightingStarted)
                    {
                        highlightingStarted = false;     
                        if (stopWatch.IsRunning)
                        {
                            stopWatch.Stop();
                            stopWatch.Reset();
                        }
                        Debug.Log("leftup 11");
                        ws.Send($"leftup");
                    } else
                    {
                        SendSingleClick(touch, null, "1");
                    }
                    beganOne = false;
                    touchCount = 0;
                    break;
            }

        }
        else if ((touchCount == 2 && touchCount != buttons.Count) || (touchCount - buttons.Count == 2))
        {
            Touch touch1 = Input.GetTouch(touchCount - 2);
            Touch touch2 = Input.GetTouch(touchCount - 1);
           // Debug.Log("val1: " + touch1.ToString() + "val2: " + touch2.ToString());

            var touchOnePosX = (int)touch1.position.x;
            var touchOnePosY = (int)touch1.position.y;
            var touchTwoPosX = (int)touch2.position.x;
            var touchTwoPosY = (int)touch2.position.y;

            switch (touch2.phase)
            {
                case TouchPhase.Began:
                    oldPositionTwoX = touchTwoPosX;
                    oldPositionTwoY = touchTwoPosY;

                    startPositionTwoX = touchTwoPosX;
                    startPositionTwoY = touchTwoPosY;
                    startPositionOneX = touchOnePosX; 
                    startPositionOneY = touchOnePosY;
                    stopWatch.Start();
                    Debug.Log("BEGAN 2 --- " + stopWatch.IsRunning);
                    beganTwo = true;
                    break;

                case TouchPhase.Moved:
                    //Debug.Log("XX: " + startPositionOneX + ", " + (int)touch1.position.x + "YY: "+ startPositionOneY + ", " + (int)touch1.position.y);
                    if (IsInRange(startPositionOneX, (int)touch1.position.x - 8, (int)touch1.position.x + 8) && IsInRange(startPositionOneY, (int)touch1.position.y - 8, (int)touch1.position.y + 8) 
                        && !(startedHorizontalScroll || startedVerticalScroll || zoomStarted || Mathf.Abs(startPositionOneY - startPositionTwoY) < 8 || Mathf.Abs(startPositionOneX - startPositionTwoX) < 8))
                    {
                        if (!highlightingStarted)
                        {
                            Debug.Log("left " + Input.touchCount);
                            ws.Send("left");
                        }
                        
                        highlightingStarted = true;

                        if (oldPositionTwoX < touchTwoPosX)
                        {
                            sendX = 1;
                        }
                        else if (oldPositionTwoX > touchTwoPosX)
                        {
                            sendX = -1;
                        }
                        else
                        {
                            sendX = 0;
                        }

                        if (oldPositionTwoY < touchTwoPosY)
                        {
                            sendY = 1;
                        }
                        else if (oldPositionTwoY > touchTwoPosY)
                        {
                            sendY = -1;
                        }
                        else
                        {
                            sendY = 0;
                        }

                        if (sendX != 0 || sendY != 0)
                        {
                            var sensX = Mathf.Abs(oldPositionTwoX - touchTwoPosX);
                            var sensY = Mathf.Abs(oldPositionTwoY - touchTwoPosY);
                            if (sensX > 50)
                            {
                                sensX += (int)(90 * 1.2);
                                if (sensY < 15)
                                {
                                    sensX += 50;
                                }
                            }
                            if (sensY >= 30)
                            {
                                sensY += (int)(90 * 1.2);
                                if (sensX < 15)
                                {
                                    sensY += 20;
                                }
                            }
                            Debug.Log("SEND MARK> X: " + (sendX * (sensX + sensY)) + " Y:" + (-sendY * (sensY + sensY)));

                            ws.Send($"mark,{sendX * (sensX + sensY)},{-sendY * (sensY + sensY)}");

                        }
                    } else 
                    {
                        if (highlightingStarted && !zoomStarted && !(startedHorizontalScroll || startedVerticalScroll))
                        {
                            highlightingStarted = false;
                            if (stopWatch.IsRunning)
                            {
                                stopWatch.Stop();
                                stopWatch.Reset();
                            }
                            Debug.Log("leftup 22");
                            ws.Send($"leftup");
                            return;
                        }

                       // Debug.Log("ONE:  " + startPositionOneY + ", " + oldPositionOneY + ", " + touchOnePosY);
                       // Debug.Log("TWO:  " + startPositionTwoY + ", " + oldPositionTwoY + ", " + touchTwoPosY);
                       // Debug.Log("ONE:  " + startPositionOneX + ", " + oldPositionOneX + ", " + touchOnePosX);
                       // Debug.Log("TWO:  " + startPositionTwoX + ", " + oldPositionTwoX + ", " + touchTwoPosX);

                       // Debug.Log((startPositionOneX - oldPositionOneX) > (startPositionOneX - touchOnePosX) && (startPositionTwoX - oldPositionTwoX) < (startPositionTwoX - touchTwoPosX) && !(startedHorizontalScroll || startedVerticalScroll || highlightingStarted));
                       // Debug.Log((startPositionOneX - oldPositionOneX) < (startPositionOneX - touchOnePosX) && (startPositionTwoX - oldPositionTwoX) > (startPositionTwoX - touchTwoPosX) && !(startedHorizontalScroll || startedVerticalScroll || highlightingStarted));

                        var startTouchDiffOneX = Mathf.Abs(startPositionOneX - touchOnePosX) > 15;
                        var startTouchDiffTwoX = Mathf.Abs(startPositionTwoX - touchTwoPosX) > 15;
                        var startTouchDiffOneY = Mathf.Abs(startPositionOneY - touchOnePosY) > 15;
                        var startTouchDiffTwoY = Mathf.Abs(startPositionTwoY - touchTwoPosY) > 15;
                        var zoomin = (startPositionOneX - oldPositionOneX) > (startPositionOneX - touchOnePosX) && (startPositionTwoX - oldPositionTwoX) < (startPositionTwoX - touchTwoPosX);
                        var zoomout = (startPositionOneX - oldPositionOneX) < (startPositionOneX - touchOnePosX) && (startPositionTwoX - oldPositionTwoX) > (startPositionTwoX - touchTwoPosX);
                        var scrollright = (startPositionOneX - oldPositionOneX) < (startPositionOneX - touchOnePosX) && (startPositionTwoX - oldPositionTwoX) < (startPositionTwoX - touchTwoPosX);
                        var scrollleft = (startPositionOneX - oldPositionOneX) > (startPositionOneX - touchOnePosX) && (startPositionTwoX - oldPositionTwoX) > (startPositionTwoX - touchTwoPosX);


                        if (!zoomStarted && startTouchDiffOneX && startTouchDiffTwoX && !startedVerticalScroll && !startedHorizontalScroll && (zoomin || zoomout))
                        {
                            zoomStarted = true;
                        }
                        else if (!startedVerticalScroll && startTouchDiffOneY && startTouchDiffTwoY && !zoomStarted && !startedHorizontalScroll)
                        {
                            startedVerticalScroll = true;
                        } else if (!startedHorizontalScroll && startTouchDiffTwoX && startTouchDiffOneX && !startedVerticalScroll && !zoomStarted && (scrollright || scrollleft))
                        {
                            startedHorizontalScroll = true;
                        }

                       // Debug.Log("BOOL: " + zoomStarted + startedHorizontalScroll + startedVerticalScroll + highlightingStarted);

                        if (!startedVerticalScroll && !highlightingStarted && zoomStarted && zoomin)
                        {
                           
                            if (oldPositionOneX < oldPositionTwoX)
                            {
                                Debug.Log("zoomout");
                                ws.Send("zoomout");

                            } else
                            {
                                Debug.Log("zoomin");
                                ws.Send("zoomin");
                            }
                               
                        } else if (!startedVerticalScroll && !highlightingStarted && zoomStarted && zoomout)
                        {
                            if (oldPositionOneX > oldPositionTwoX)
                            {
                                Debug.Log("zoomout");
                                ws.Send("zoomout");
                            }
                            else
                            {
                                Debug.Log("zoomin");
                                ws.Send("zoomin");
                            }
                        } else if ((startPositionOneY - oldPositionOneY) > (startPositionOneY - touchOnePosY) && (startPositionTwoY - oldPositionTwoY) > (startPositionTwoY - touchTwoPosY) && !highlightingStarted && !zoomStarted && !startedHorizontalScroll && startedVerticalScroll)
                        {
                            var sensY = Mathf.Abs(oldPositionTwoY - touchTwoPosY);
                            if (sensY >= 30)
                            {
                                sensY -= 15;
                            } else
                            {
                                sensY = 1;
                            }
                            Debug.Log("scrolldown " + sensY);
                            ws.Send($"scrolldown,{sensY}");
                        } else if ((startPositionOneY - oldPositionOneY) < (startPositionOneY - touchOnePosY) && (startPositionTwoY - oldPositionTwoY) < (startPositionTwoY - touchTwoPosY) && !highlightingStarted && !zoomStarted && !startedHorizontalScroll && startedVerticalScroll)
                        {
                            var sensY = Mathf.Abs(oldPositionTwoY - touchTwoPosY);
                            if (sensY >= 30)
                            {
                                sensY -= 15;
                            } else
                            {
                                sensY = 1;
                            }
                            Debug.Log("scrollup " + sensY);
                            ws.Send($"scrollup,{sensY}");
                        }
                        else if (startedHorizontalScroll && scrollright && !highlightingStarted && !zoomStarted && !startedVerticalScroll)
                        {
                            Debug.Log("scrollright");
                            ws.Send($"scrollright");
                        }
                        else if (startedHorizontalScroll && scrollleft && !highlightingStarted && !zoomStarted && !startedVerticalScroll)
                        {
                            Debug.Log("scrollleft");
                            ws.Send($"scrollleft");
                        }
                    }

                    oldPositionOneX = touchOnePosX;
                    oldPositionOneY = touchOnePosY;
                    oldPositionTwoX = touchTwoPosX;
                    oldPositionTwoY = touchTwoPosY;
                    break;

                case TouchPhase.Ended:
                    if (!highlightingStarted && !startedVerticalScroll && !startedHorizontalScroll && !zoomStarted)
                    {
                        SendSingleClick(touch1, touch2, "2");
                    }
                    startedVerticalScroll = false;
                    startedHorizontalScroll = false;
                    touchCount = 0;
                    zoomStarted = false;
                    beganTwo = false;
                    Debug.Log("twoended");
                    break;
            }

        }

    }

    private void SendSingleClick(Touch t, Touch? t2 = null, string from = "")
    {
        if (ws != null && !ws.IsAlive)
            return;

        if (startedHorizontalScroll || startedVerticalScroll || zoomStarted || cancelClick)
        {
            if (stopWatch.IsRunning)
            {
                stopWatch.Stop();
                stopWatch.Reset();
            }
            return;
        }
        
        TimeSpan ts = stopWatch.Elapsed;
        //Debug.Log(" hasval=" + t2.HasValue + " count=" + Input.touchCount + " from=" + from);
        if (!t2.HasValue)
        {
            if (t.phase == TouchPhase.Ended && ts.Milliseconds < 200)
            {
                Debug.Log("left " + Input.touchCount + " " + ts.Milliseconds);
                ws.Send("left");
            }
            else if (t.phase == TouchPhase.Ended && ts.Milliseconds >= 201 && IsInRange((int)t.position.x, startPositionOneX - 8, startPositionOneX + 8) && IsInRange((int)t.position.y, startPositionOneY - 8, startPositionOneY + 8) /*oldPositionOneX == (int)t.position.x && oldPositionOneY == (int)t.position.y*/)
            {
                Debug.Log("right " + Input.touchCount + " " + ts.Milliseconds);
                ws.Send("right");
            }
        } else
        {
            if (t.phase == TouchPhase.Ended && t2.Value.phase == TouchPhase.Ended && ts.Milliseconds < 250)
            {
                if (Input.touchCount == 1)
                    return;

                Debug.Log("right2 " + Input.touchCount + " " + ts.Milliseconds);
                ws.Send("right");
            }
        }
        stopWatch.Stop();
        stopWatch.Reset();
    }

    private bool IsInRange(int numberToCheck, int bottom, int top)
    {
        return (numberToCheck >= bottom && numberToCheck <= top);
    }
    
    public void OpetWebSocket()
    {
        if (string.IsNullOrWhiteSpace(portInput.text) || string.IsNullOrWhiteSpace(hostInput.text))
            return;

        var host = Dns.GetHostEntry(Dns.GetHostName());
        var networkID = "";
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                var str = ip.ToString();
                int index = str.LastIndexOf(".");
                networkID = str.Substring(0, index + 1);
            }
        }

        ws = new WebSocket("ws://" + networkID + hostInput.text + ":" + portInput.text);

        try
        {
            ws.Connect();
            
            Debug.Log("IS CONNECTED " + ws.IsAlive);
            isConn.text = ws.IsAlive.ToString();

            ws.OnOpen += Ws_OnOpen;
            ws.OnClose += Ws_OnClose;
            ws.OnError += Ws_OnError;
            ws.OnMessage += Ws_OnMessage;
            empty1MoveDown = true;
        }
        catch (System.Exception e)
        {
            Debug.Log("Error EXCEPTION" + e.Message);
        }
    }

    private void Ws_OnMessage(object sender, MessageEventArgs e)
    {
        Debug.Log("OnMessage");
    }

    private void Ws_OnError(object sender, ErrorEventArgs e)
    {
        //Debug.Log("OnError");
    }

    private void Ws_OnClose(object sender, CloseEventArgs e)
    {
        Debug.Log("server OnClose");
    }

    private void Ws_OnOpen(object sender, System.EventArgs e)
    {
        Debug.Log("OnOpen");
    }

    public void btnOnPressHold(string name)
    {
        buttons.Add(name);
    }

    public void bntOnPressRelease(string name)
    {
        ws.Send("release," + name);
        buttons.Remove(name);
    }

    public void OpenKeyboard()
    {
        if (ws != null && ws.IsAlive)
        {
            keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false);
        }
    }

    private bool IsPointerOverUIObject(int touchcount)
    {
        list.Clear();
        
        for (int i = 0; i < touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            var hits = Physics2D.RaycastAll(t.position, transform.forward);

            list.AddRange(hits);
        }

        if (list.Count(x => x.transform.tag == "HelperButton") >= touchcount) return true;
        else if (list.Count(x => x.transform.tag == "SettingsPanel") > 0) return true;
        else return false;
    }

}
