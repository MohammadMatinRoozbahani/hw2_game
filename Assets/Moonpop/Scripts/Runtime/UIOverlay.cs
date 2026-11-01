using System;
using Moonpop.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Moonpop
{
 public sealed class UIOverlay:MonoBehaviour
 {
  private readonly Color ink=new Color(.13f,.12f,.25f),panel=new Color(.13f,.12f,.25f,.93f),muted=new Color(.69f,.69f,.81f),cream=new Color(1,.94f,.83f),yellow=new Color(1,.81f,.36f),mint=new Color(.38f,.94f,.77f),berry=new Color(1,.43f,.60f);
  private GameManager game;private Font regular,bold,mono;private Sprite round,heart;private Transform frame;
  private GameObject hud,menu,pause,over;private Button startButton,resumeButton,restartButton,pauseButton,cameraButton;
  private Text score,best,stars,speed,fuelText,nitroText,notice,challenge,menuRecord,skinName,qualityText,calmText,soundText,endScore,endBest,endStats,endReason,cameraText,pauseHint;
  private Image fuelFill,nitroFill,noticePlate,flash;private readonly Image[] hearts=new Image[3];private readonly Image[] skinChips=new Image[4];
  private float noticeTime,flashAlpha,refreshTimer;private bool ready;
  public bool MenuVisible=>menu&&menu.activeSelf;
  public bool GameOverVisible=>over&&over.activeSelf;
  public void Initialize(GameManager gm)
  {
   game=gm;regular=Resources.Load<Font>("Fonts/Regular");bold=Resources.Load<Font>("Fonts/Bold");mono=Resources.Load<Font>("Fonts/Mono");
   round=RoundedSprite();heart=HeartSprite();
   var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;
   var scaler=gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
   gameObject.AddComponent<GraphicRaycaster>();
   var design=new GameObject("Design frame",typeof(RectTransform));design.transform.SetParent(transform,false);var rect=(RectTransform)design.transform;
   rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.pivot=new Vector2(.5f,.5f);rect.sizeDelta=new Vector2(1280,720);frame=design.transform;
   BuildHud();BuildMenu();BuildPause();BuildOver();flash=Full("Damage tint",frame,new Color(1,.3f,.5f,0),false);ready=true;RefreshGarage();
  }
  private void BuildMenu()
  {
   menu=Group("Main menu",frame);Full("Gentle night shade",menu.transform,new Color(.04f,.03f,.10f,.12f),true);
   Card(menu.transform,42,86,522,574,panel);Card(menu.transform,60,46,172,32,new Color(.31f,.24f,.46f));
   Label(menu.transform,"A HAPPY LITTLE NIGHT DRIVE",70,49,155,25,10,cream,regular,TextAnchor.MiddleCenter);
   Label(menu.transform,"MOONPOP",73,124,455,80,65,cream,bold);
   Label(menu.transform,"night parade",78,208,430,47,34,mint,bold);
   Label(menu.transform,"A tiny car. A sky full of fireworks.\nLet's make some happy little miles.",80,276,420,54,16,new Color(.85f,.85f,.92f),regular);
   startButton=Button(menu.transform,"LET'S ROLL!   >",78,354,414,57,game.StartRun,yellow,ink,20);
   Label(menu.transform,"PICK YOUR LITTLE BUDDY",80,437,410,22,11,muted,regular);
   for(int i=0;i<4;i++)
   {
    int index=i;ArtMaterial m=ArtRecipe.Materials()[i];Color tint=new Color(m.color.x,m.color.y,m.color.z);
    var b=Button(menu.transform,new[]{"MINT","BERRY","LILAC","LEMON"}[i],78+i*105,470,95,44,()=>game.SelectSkin(index),tint,ink,12);skinChips[i]=b.GetComponent<Image>();
   }
   qualityText=Button(menu.transform,"",78,537,200,35,game.ToggleQuality,new Color(.24f,.22f,.38f),cream,11).GetComponentInChildren<Text>();
   soundText=Button(menu.transform,"",292,537,200,35,game.ToggleMute,new Color(.24f,.22f,.38f),cream,11).GetComponentInChildren<Text>();
   calmText=Button(menu.transform,"",78,581,200,35,game.ToggleCalm,new Color(.24f,.22f,.38f),cream,11).GetComponentInChildren<Text>();
   menuRecord=Label(menu.transform,"BEST  000000",298,585,194,26,16,yellow,bold,TextAnchor.MiddleRight);
   Button(menu.transform,"QUIT",1157,27,93,36,game.Quit,new Color(.18f,.16f,.3f,.9f),cream,13);
   Label(menu.transform,"01 / THE NIGHT PARADE",864,35,267,22,12,cream,regular,TextAnchor.MiddleRight);
   Card(menu.transform,844,604,350,53,new Color(1,.94f,.83f,.95f));
   skinName=Label(menu.transform,"Meet Mint Mochi",856,613,326,32,24,ink,bold,TextAnchor.MiddleCenter);
   Label(menu.transform,"A / D  STEER       SHIFT / SPACE  BOOST       C  CAMERA",63,677,740,20,11,cream,regular);
  }
  private void BuildHud()
  {
   hud=Group("Driving HUD",frame);
   Card(hud.transform,20,18,280,104,panel);Label(hud.transform,"MOONPOP",36,28,170,20,14,mint,bold);
   score=Label(hud.transform,"000000",34,52,170,43,33,cream,bold);best=Label(hud.transform,"BEST 000000",36,99,190,16,10,muted,mono);
   stars=Label(hud.transform,"0",209,60,72,30,22,yellow,bold,TextAnchor.MiddleRight);
   Label(hud.transform,"STARS",212,91,66,16,9,yellow,regular,TextAnchor.MiddleRight);
   Card(hud.transform,317,20,146,67,panel);Label(hud.transform,"LITTLE LIVES",333,27,114,17,10,muted,regular);
   for(int i=0;i<3;i++)
   {
    hearts[i]=Card(hud.transform,333+i*35,50,26,23,berry);hearts[i].sprite=heart;hearts[i].type=Image.Type.Simple;
   }
   Card(hud.transform,482,20,305,67,panel);Label(hud.transform,"FUEL RESERVE",500,29,180,18,10,muted,regular);
   fuelText=Label(hud.transform,"100%",696,27,72,21,13,cream,bold,TextAnchor.MiddleRight);
   Card(hud.transform,500,58,269,10,new Color(.27f,.25f,.39f));fuelFill=Card(hud.transform,500,58,269,10,mint);
   cameraButton=Button(hud.transform,"CAM",1108,20,65,42,game.CycleCamera,panel,cream,12);
   pauseButton=Button(hud.transform,"II",1185,20,65,42,game.TogglePause,panel,cream,17);
   cameraText=Label(hud.transform,"CHASE",1090,72,156,18,9,cream,regular,TextAnchor.MiddleRight);
   Card(hud.transform,20,595,280,99,panel);Label(hud.transform,"POPCORN BOOST",38,608,200,20,12,yellow,bold);
   nitroText=Label(hud.transform,"HOLD SHIFT / SPACE",38,663,242,18,10,muted,regular);
   Card(hud.transform,38,642,244,10,new Color(.27f,.25f,.39f));nitroFill=Card(hud.transform,38,642,244,10,yellow);
   Card(hud.transform,1093,578,157,116,panel);Label(hud.transform,"HAPPY SPEED",1109,590,126,20,10,mint,regular);
   speed=Label(hud.transform,"094",1107,610,125,53,39,cream,bold,TextAnchor.MiddleCenter);Label(hud.transform,"KM / H",1110,667,122,17,10,muted,regular,TextAnchor.MiddleCenter);
   Card(hud.transform,405,652,470,41,panel);challenge=Label(hud.transform,"COLLECT 20 STARS",420,660,440,25,12,cream,regular,TextAnchor.MiddleCenter);
   noticePlate=Card(hud.transform,355,133,570,41,new Color(.16f,.14f,.28f,.94f));notice=Label(hud.transform,"",367,139,546,29,17,yellow,bold,TextAnchor.MiddleCenter);
  }
  private void BuildPause()
  {
   pause=Group("Pause menu",frame);Full("Pause backdrop",pause.transform,new Color(.035f,.025f,.09f,.70f),true);
   Card(pause.transform,407,125,466,469,cream);Label(pause.transform,"A TINY PIT STOP",439,160,402,25,12,new Color(.48f,.40f,.59f),regular,TextAnchor.MiddleCenter);
   Label(pause.transform,"your parade can wait",430,202,420,54,34,ink,bold,TextAnchor.MiddleCenter);
   pauseHint=Label(pause.transform,"ESC / P or click below to continue.",439,262,402,52,14,new Color(.45f,.39f,.56f),regular,TextAnchor.MiddleCenter);
   resumeButton=Button(pause.transform,"BACK TO THE PARADE",449,328,382,53,game.TogglePause,mint,ink,18);
   Button(pause.transform,"FRESH START",449,394,382,50,game.StartRun,new Color(.88f,.82f,.96f),ink,17);
   Button(pause.transform,"MAIN MENU",449,457,382,50,game.ReturnToMenu,new Color(.88f,.82f,.96f),ink,17);
   Label(pause.transform,"ESC  RESUME       R  RESTART",443,547,394,20,11,new Color(.48f,.40f,.59f),regular,TextAnchor.MiddleCenter);
  }
  private void BuildOver()
  {
   over=Group("Game over",frame);Full("Game over backdrop",over.transform,new Color(.035f,.025f,.09f,.75f),true);
   Card(over.transform,386,62,508,590,cream);Label(over.transform,"EVERY LITTLE RIDE COUNTS",423,87,434,23,12,new Color(.48f,.4f,.59f),regular,TextAnchor.MiddleCenter);
   Label(over.transform,"cute try, little buddy!",414,125,452,50,33,ink,bold,TextAnchor.MiddleCenter);
   endReason=Label(over.transform,"OUT OF FUEL",430,189,420,23,13,new Color(.8f,.24f,.43f),regular,TextAnchor.MiddleCenter);
   Label(over.transform,"YOUR HAPPY SCORE",431,235,418,22,11,new Color(.48f,.40f,.59f),regular,TextAnchor.MiddleCenter);
   endScore=Label(over.transform,"000000",426,265,428,69,53,ink,bold,TextAnchor.MiddleCenter);
   endBest=Label(over.transform,"PERSONAL BEST",428,349,424,23,15,new Color(.22f,.55f,.45f),bold,TextAnchor.MiddleCenter);
   endStats=Label(over.transform,"",423,390,434,50,12,new Color(.48f,.40f,.59f),regular,TextAnchor.MiddleCenter);
   restartButton=Button(over.transform,"ONE MORE LITTLE RIDE!",429,473,422,52,game.StartRun,yellow,ink,19);
   Button(over.transform,"MAIN MENU",429,540,422,44,game.ReturnToMenu,new Color(.88f,.82f,.96f),ink,16);
   Label(over.transform,"R  RESTART       BACKSPACE  MENU",425,611,430,18,10,new Color(.48f,.40f,.59f),regular,TextAnchor.MiddleCenter);
  }
  public void ShowState(GameState state)
  {
   menu.SetActive(state==GameState.Menu);pause.SetActive(state==GameState.Paused);over.SetActive(state==GameState.GameOver);hud.SetActive(state!=GameState.Menu);
   pauseButton.interactable=cameraButton.interactable=state==GameState.Playing;
   if(state==GameState.Paused)pauseHint.text="ESC / P or click below to continue.";
   if(state==GameState.Menu||state==GameState.Playing){noticeTime=0;flashAlpha=0;}
   if(state==GameState.GameOver)
   {
    var s=game.Session;endReason.text=s.Reason==EndReason.FuelEmpty?"OUT OF FUEL - MOCHI NEEDS A REFILL":"THREE COLLISIONS - TIME FOR A LITTLE REST";
    endScore.text=s.Score.ToString("D6");endBest.text=(s.Score>game.RecordAtStart?"NEW PERSONAL BEST!  ":"PERSONAL BEST  ")+s.BestScore.ToString("D6");
    endStats.text=(s.Distance/1000).ToString("0.00")+" KM   /   "+s.Coins+" STARS   /   "+s.NearMisses+" CLOSE CALLS\nBEST COMBO x"+s.BestCombo+"    |    "+CountBits(s.Badges)+" / 3 STICKERS";
   }
   if(EventSystem.current)
   {
    EventSystem.current.SetSelectedGameObject(null);Button first=state==GameState.Menu?startButton:state==GameState.Paused?resumeButton:state==GameState.GameOver?restartButton:null;
    if(first)EventSystem.current.SetSelectedGameObject(first.gameObject);
   }
   Refresh();
  }
  public void RefreshGarage()
  {
   if(!ready)return;skinName.text="Meet "+ArtRecipe.Skins[game.SelectedSkin];
   qualityText.text=game.LowSpec?"QUALITY: COZY LITE":"QUALITY: SOFT SHADOWS";calmText.text=game.ReducedMotion?"CALM CAMERA: ON":"CALM CAMERA: OFF";
   for(int i=0;i<4;i++){var outline=skinChips[i].GetComponent<Outline>();if(!outline)outline=skinChips[i].gameObject.AddComponent<Outline>();outline.effectColor=cream;outline.effectDistance=i==game.SelectedSkin?new Vector2(3,-3):Vector2.zero;}
   RefreshSound();
  }
  public void RefreshSound(){if(soundText)soundText.text=game.Audio.Muted?"SOUND: OFF":"SOUND: ON";}
  public void ShowPauseHint(string message){pauseHint.text=message;}
  public void Notify(string message,Color color){notice.text=message;notice.color=color;noticeTime=2.7f;}
  public void Damage(){flashAlpha=.19f;}
  private void Update()
  {
   if(!ready)return;refreshTimer-=Time.unscaledDeltaTime;if(refreshTimer<=0){Refresh();refreshTimer=.1f;}
   noticeTime-=Time.unscaledDeltaTime;notice.enabled=noticePlate.enabled=noticeTime>0;
   flashAlpha=Mathf.MoveTowards(flashAlpha,0,Time.unscaledDeltaTime*.65f);flash.color=new Color(1,.23f,.43f,flashAlpha);
  }
  private void Refresh()
  {
   var s=game.Session;score.text=s.Score.ToString("D6");best.text="BEST "+s.BestScore.ToString("D6");stars.text=s.Coins.ToString();
   speed.text=Mathf.RoundToInt(s.State==GameState.Menu?0:s.Speed*3.6f).ToString("D3");
   float f=s.Fuel/s.Rules.MaximumFuel;fuelFill.rectTransform.sizeDelta=new Vector2(269*f,10);fuelFill.color=f<.25f?berry:mint;fuelText.text=Mathf.CeilToInt(f*100)+"%";
   nitroFill.rectTransform.sizeDelta=new Vector2(244*s.Nitro/100,10);nitroText.text=s.Boosting?"WHEEEE!  "+Mathf.RoundToInt(s.Nitro)+"%":"HOLD SHIFT / SPACE    "+Mathf.RoundToInt(s.Nitro)+"%";
   for(int i=0;i<3;i++)hearts[i].color=i<s.Lives?berry:new Color(.34f,.30f,.44f);
   cameraText.text=game.CameraRig.ModeName+" / C";menuRecord.text="BEST  "+s.BestScore.ToString("D6");
   if((s.Badges&1)==0)challenge.text="STICKER 1 / COLLECT 20 STARS    "+Mathf.Min(s.Coins,20)+" / 20";
   else if((s.Badges&2)==0)challenge.text="STICKER 2 / 8 CLOSE CALLS    "+Mathf.Min(s.NearMisses,8)+" / 8";
   else if((s.Badges&4)==0)challenge.text="STICKER 3 / DRIVE 1.5 KM    "+(s.Distance/1000).ToString("0.00")+" KM";
   else challenge.text="ALL 3 STICKERS!   YOU MADE MOCHI'S NIGHT.";
  }
  private static int CountBits(int value){int count=0;while(value!=0){count+=value&1;value>>=1;}return count;}
  private GameObject Group(string name,Transform parent)
  {
   var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var r=(RectTransform)go.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return go;
  }
  private RectTransform Place(GameObject go,Transform parent,float x,float y,float w,float h)
  {
   go.transform.SetParent(parent,false);var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
  }
  private Image Card(Transform parent,float x,float y,float w,float h,Color color)
  {
   var go=new GameObject("Soft card",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));Place(go,parent,x,y,w,h);
   var image=go.GetComponent<Image>();image.sprite=round;image.type=Image.Type.Sliced;image.color=color;image.raycastTarget=false;return image;
  }
  private Image Full(string name,Transform parent,Color color,bool block){var go=Group(name,parent);var i=go.AddComponent<Image>();i.color=color;i.raycastTarget=block;return i;}
  private Text Label(Transform parent,string text,float x,float y,float w,float h,int size,Color color,Font font,TextAnchor alignment=TextAnchor.MiddleLeft)
  {
   var go=new GameObject(text,typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));Place(go,parent,x,y,w,h);var t=go.GetComponent<Text>();t.text=text;t.font=font;t.fontSize=size;t.color=color;t.alignment=alignment;t.raycastTarget=false;t.supportRichText=true;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;return t;
  }
  private Button Button(Transform parent,string text,float x,float y,float w,float h,Action click,Color fill,Color textColor,int size)
  {
   var image=Card(parent,x,y,w,h,Color.white);image.name=text;image.raycastTarget=true;var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;
   var colors=b.colors;colors.normalColor=fill;colors.highlightedColor=Color.Lerp(fill,Color.white,.17f);colors.selectedColor=colors.highlightedColor;colors.pressedColor=Color.Lerp(fill,ink,.15f);colors.disabledColor=new Color(.24f,.21f,.34f);colors.fadeDuration=.08f;b.colors=colors;
   Label(image.transform,text,8,0,w-16,h,size,textColor,bold,TextAnchor.MiddleCenter);b.onClick.AddListener(()=>{game.Audio.PlayClick();click();});return b;
  }
  private static Sprite RoundedSprite()
  {
   const int n=64;var tex=new Texture2D(n,n,TextureFormat.RGBA32,false);tex.filterMode=FilterMode.Bilinear;tex.wrapMode=TextureWrapMode.Clamp;
   for(int y=0;y<n;y++)for(int x=0;x<n;x++)
   {
    float dx=Mathf.Max(0,Mathf.Abs(x-31.5f)-15.5f),dy=Mathf.Max(0,Mathf.Abs(y-31.5f)-15.5f);float alpha=Mathf.Clamp01(16.3f-Mathf.Sqrt(dx*dx+dy*dy));tex.SetPixel(x,y,new Color(1,1,1,alpha));
   }
   tex.Apply();return Sprite.Create(tex,new Rect(0,0,n,n),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(18,18,18,18));
  }
  private static Sprite HeartSprite()
  {
   const int n=64;var tex=new Texture2D(n,n,TextureFormat.RGBA32,false);tex.filterMode=FilterMode.Bilinear;
   for(int y=0;y<n;y++)for(int x=0;x<n;x++)
   {
    float a=(x-31.5f)/27,b=(y-30f)/27;float q=a*a+b*b-1;float value=q*q*q-a*a*b*b*b;tex.SetPixel(x,y,new Color(1,1,1,value<=0?1:0));
   }
   tex.Apply();return Sprite.Create(tex,new Rect(0,0,n,n),new Vector2(.5f,.5f));
  }
 }
}
