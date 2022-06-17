using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using AdvanceMath.Design;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;


/// @author Teemu Volanen
/// @version 140121
///
/// <summary>
/// Harjoitustyö ohjelmointi 1 kursille. Tarkoituksena on tehdä peli jypeli-peliohjelmointikirjaston avulla. Tämän
/// pelin nimi on Eeeppisen Avaruussankarin Uskomattomat Seikkailut. Pelin tarkoituksena on paeta vihamieliseltä
/// planeetalta. Pelissä saa pisteitä jokaisesta alienista, jonka tuhoaa. Pelin kuvat olen piirtänyt itse.
/// Sankarin plasmapyssyn ääni oli valmiiksi, mutta muut pelin äänet on ladattu internetsivulta freesound.org
/// (Credits to SRJA_Gaming, Cabeeno Rossley and luckylittleraven).
/// </summary>
public class EeppisenAvaruussankarinUskomattomatSeikkailut : PhysicsGame
{
    private const int RUUDUN_KOKO = 40;
    private PlatformCharacter sankari;
    private IntMeter elamat = new IntMeter(3);
    private IntMeter pisteet = new IntMeter(0);
    private List<int> alienLkm = new List<int>();
    private EasyHighScore topLista = new EasyHighScore();
    private int kenttaNro = 1; //Uusi rivi 140121

    private readonly Image sankarinKuva = LoadImage("sankari.png");
    private readonly Image alusKuva = LoadImage("avaruusalus.png");
    private readonly Image tasoKuva1 = LoadImage("maaaines1.png");
    private readonly Image ammusKuva = LoadImage("tuliammus.png");
    private readonly Image alienKuva = LoadImage("alien2.png");
    private readonly Image laavaKuva = LoadImage("laava.png");
    private readonly Image tasoKuva2 = LoadImage("maaaines2.png");
    private readonly Image tasoKuva3 = LoadImage("maaaines3.png");
    private readonly Image tasoKuva4 = LoadImage("maaaines4.png");
    private readonly Image tasoKuva5 = LoadImage("maaaines5.png");
    private readonly Image tasoKuva6 = LoadImage("maaaines6.png");
    private readonly Image tasoKuva7 = LoadImage("maaaines7.png");
    private readonly Image tasoKuva8 = LoadImage("maaaines8.png");
    private readonly Image tasoKuva9 = LoadImage("maaaines9.png"); //LoadImages metodilla montakuvaa yhteen taulukkoon.

    private SoundEffect maaliAani = LoadSoundEffect("voitto.wav"); //126422_cabeeno-rossley_level-up.wav
    private SoundEffect hyppyAani = LoadSoundEffect("hyppy.wav"); //126416_cabeeno-rossley_jump.wav
    private SoundEffect kipuAani = LoadSoundEffect("kipu.wav"); //544887_srja-gaming_retro-video-game-damaged-effect.wav
    private SoundEffect kuolemaAani = LoadSoundEffect("kuolema.wav"); //543255_srja-gaming_retro-video-game-death.wav
    private SoundEffect alienKuoleeAani = LoadSoundEffect("piste.wav"); //543270_srja-gaming_retro-video-game-coin-collect.wav
    
    /// <summary>
    /// Aliohjelma, jossa aloitetaan peli, luodaan kenttä, hahmot, näppäimet ja esim. painovoima.
    /// </summary>
    public override void Begin()
    {
        topLista.Clear();
        ClearAll();
        //AloitusValikko();  Tämä rivi oli 261120 versiosta
        SeuraavaKentta();
        //LuoKentta(); Tämä rivi oli 261120 versiosta
        //LuoNaytto("Elämät", elamat, 80);  Tämä rivi oli 261120 versiosta
        //LuoNaytto("Pisteet", pisteet, 110);  Tämä rivi oli 261120 versiosta
        //LuoSydamet();
        //LisaaNappaimet(); Tämä rivi oli 261120 versiosta

        MediaPlayer.Play("taustamusiikki.wav"); // 202138__luckylittleraven__thepact2.wav 
        MediaPlayer.IsRepeating = true;
        
        //Camera.Follow(sankari); Tämä rivi oli 261120 versiosta
        //Camera.ZoomFactor = 1.5; Tämä rivi oli 261120 versiosta
        //Camera.StayInLevel = true; Tämä rivi oli 261120 versiosta
        
        //Gravity = new Vector(0, -1000); Tämä rivi oli 261120 versiosta
        elamat.Value = 3;
    }

    
    /// <summary>
    /// Aliohjelma, joka laskee pelin päätyttyä pelaajan pisteet prosentteina.
    /// </summary>
    /// <returns>pelaajan lopulliset pisteet</returns>
    private double LoppuPisteet()
    {
        double eka = pisteet.Value;
        double toka = alienLkm.Count;
        if (toka == 0) return 100;
        double tulos = (eka / toka) * 100;
        return tulos;
    }

    
    /// <summary>
    /// Aliohjelma, jossa luodaan pelin aloitusvalikko.
    /// </summary>
    private void AloitusValikko()
    {
        Widget ohje = PelinLogo();
        MultiSelectWindow valikko = new MultiSelectWindow("OLETKO VALMIS?", "ALOITA PELI", "LOPETA PELI");
        valikko.Position = new Vector(0, -200);
        valikko.AddItemHandler(0, () => { ohje.Destroy(); });
        valikko.AddItemHandler(1, Exit);
        valikko.Color = Color.AshGray;
        Add(valikko);
    }

    
    /// <summary>
    /// Aliohjelma, jossa luodaan pelin alussa näytettävä logo.
    /// </summary>
    /// <returns>Pelin logon</returns>
    private Widget PelinLogo()
    {
        Widget logo = new Widget(700, 700);
        logo.Position = new Vector(0, 100);
        logo.Image = LoadImage("Logo2.png");
        Add(logo);
        return logo;
    }

    
    /// <summary>
    /// Aliohjelma, jossa luodaan pelivalikko erilaisiin pelin tilanteisiin.
    /// </summary>
    /// <param name="game">Peli, johon pelivalikko luodaan</param>
    /// <param name="otsikko">Pelivalikon otsikko</param>
    /// <param name="ylempi">Pelivalikon ylempi näppäin</param>
    /// <param name="alempi">Pelivalikon alempi näppäin</param>
    private void PeliValikko(PhysicsGame game, string otsikko, string ylempi, string alempi)
    {
        MultiSelectWindow valikko = new MultiSelectWindow(otsikko, ylempi, alempi);
        valikko.Position = new Vector(0, 0);
        valikko.AddItemHandler(0, Begin);
        valikko.AddItemHandler(1, Exit);
        valikko.Color = Color.AshGray;
        Add(valikko);
    }

    /// <summary>
    /// Aliohjelma, jossa luodaan seuraava kenttä riippuen siitä missä kentässä sankari on.
    /// </summary>
    private void SeuraavaKentta() // Koko metodi on uusi 140121
    {
        int kenttienLkm = 3;
        if (kenttaNro < kenttienLkm + 1) ClearAll();

        if (kenttaNro > kenttienLkm) Voitit();
        else LuoKentta("kentta" + kenttaNro);
    }
    
    
    /// <summary>
    /// Aliohjelma, jossa luodaan pelin kenttä teksitiedostosta ja lasketaan alienien lukumäärä.
    /// </summary>
    private void LuoKentta(string kenttaNimi)// Tähän lisättiin kenttänimi parametri 140121
    {
        if (kenttaNro == 1) AloitusValikko();

        TileMap kentta = TileMap.FromLevelAsset(kenttaNimi); // Tässä aikaisemmin "kentta1.txt" pelkästään 261120
        kentta.SetTileMethod('#', LisaaTaso, tasoKuva1);
        kentta.SetTileMethod('2', LisaaTaso, tasoKuva2);
        kentta.SetTileMethod('3', LisaaTaso, tasoKuva3);
        kentta.SetTileMethod('4', LisaaTaso, tasoKuva4);
        kentta.SetTileMethod('5', LisaaTaso, tasoKuva5);
        kentta.SetTileMethod('6', LisaaTaso, tasoKuva6);
        kentta.SetTileMethod('7', LisaaTaso, tasoKuva7);
        kentta.SetTileMethod('8', LisaaTaso, tasoKuva8);
        kentta.SetTileMethod('9', LisaaTaso, tasoKuva9); //käytä delegaattia? Vai onko tämä tyyli hyvä?
        kentta.SetTileMethod('*', LisaaAlus);
        kentta.SetTileMethod('S', LisaaSankari);
        kentta.SetTileMethod('0', LisaaLaava);
        kentta.SetTileMethod('A', LisaaAlien);
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);

        Level.CreateBorders();
        Level.Background.CreateStars();
        
        LisaaNappaimet();
        
        LuoNaytto("Elämät", elamat, 80);
        LuoNaytto("Pisteet", pisteet, 110);

        Camera.Follow(sankari);
        Camera.ZoomFactor = 1.5;
        Camera.StayInLevel = true;

        Gravity = new Vector(0, -1000);

        int sarakkeet = kentta.ColumnCount;
        int rivit = kentta.RowCount;
        for (int i = 0; i < sarakkeet; i++)
        {
            for (int j = 0; j < rivit; j++)
            {
                char merkki = kentta.GetTile(j, i);
                if (merkki.Equals('A')) alienLkm.Add(1);
            }
        }
    }

    
    /// <summary>
    /// Aliohjelma, jossa lisätään peliin kiinteä taso.
    /// </summary>
    /// <param name="paikka">Tason sijainti</param>
    /// <param name="leveys">Tason leveys</param>
    /// <param name="korkeus">Tason korkeus</param>
    /// <param name="tasoKuva">Tason ulkonäkö</param>
    private void LisaaTaso(Vector paikka, double leveys, double korkeus, Image tasoKuva)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Image = tasoKuva;
        taso.Tag = "taso";
        Add(taso);
    }
    
    
    /// <summary>
    /// Aliohjelma, jossa luodaan peliin laavataso, johon osuessa kuolee.
    /// </summary>
    /// <param name="paikka">Laavatason sijainti</param>
    /// <param name="leveys">Laavatason leveys</param>
    /// <param name="korkeus">Laavatason Korkeus</param>
    private void LisaaLaava(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject laava = PhysicsObject.CreateStaticObject(leveys, korkeus);
        laava.Position = paikka;
        laava.Image = laavaKuva;
        laava.Tag = "laava";
        Add(laava);
    }

    
    /// <summary>
    /// Aliohjelma, jossa peliin lisätään avaruusalus, johon osuessa voittaa pelin.
    /// </summary>
    /// <param name="paikka">Avaruusaluksen sijainti</param>
    /// <param name="leveys">Avaruusaluksen leveys</param>
    /// <param name="korkeus">Avaruusaluksen korkeus</param>
    private void LisaaAlus(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject alus = PhysicsObject.CreateStaticObject(leveys, korkeus);
        alus.IgnoresCollisionResponse = true;
        alus.Position = paikka;
        alus.Image = alusKuva;
        alus.Tag = "alus";
        Add(alus);
    }

    
    /// <summary>
    /// Aliohjelma, jossa peliin lisätään sankari, jota pelaaja ohjaa. Sankarii osaa liikkua hyppiä ja ampua.
    /// </summary>
    /// <param name="paikka">Sankarin aloitus sijainti</param>
    /// <param name="leveys">Sankarin leveys</param>
    /// <param name="korkeus">Sankarin korkeus</param>
    private void LisaaSankari(Vector paikka, double leveys, double korkeus)
    {
        sankari = new PlatformCharacter(leveys, korkeus);
        sankari.Position = paikka;
        sankari.Mass = 4.0;
        sankari.Image = sankarinKuva;
        AddCollisionHandler(sankari, "alus", TormaaAlukseen);
        AddCollisionHandler(sankari, "laava", TormaaLaavaan);
        AddCollisionHandler(sankari, "alien", TormaaAlieniin);
        Add(sankari);
        
        sankari.Weapon = new PlasmaCannon(0, 0);
        sankari.Weapon.Ammo.Value = 100;
        sankari.Weapon.FireRate = 1.0;
        sankari.Weapon.X = -5;
        sankari.Weapon.Y = -10;
        sankari.Weapon.ProjectileCollision = AmmusOsui;
    }

    
    /// <summary>
    /// Aliohjelma, jossa peliin lisätään alieneita. Alieni osaa liikkua ja sankarin osuessa alieniin menettää
    /// elämäpisteen.
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    private void LisaaAlien(Vector paikka, double leveys, double korkeus)
    {
        PlatformCharacter alien = new PlatformCharacter(leveys, korkeus);
        alien.Position = paikka;
        alien.Image = alienKuva;
        alien.Tag = "alien";
        Add(alien);

        PlatformWandererBrain tasoAivot = new PlatformWandererBrain();
        tasoAivot.Speed = 100;
        alien.Brain = tasoAivot;
    }

    
    /// <summary>
    /// Aliohjelma, jossa lisätään pelin näppäimet joita painamalla peliä pelataan.
    /// </summary>
    private void LisaaNappaimet()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", sankari, -200.0);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", sankari, 200.0);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", sankari, 750.0);
        Keyboard.Listen(Key.Space, ButtonState.Down, AmmuAseella, "Pelaaja ampuu", sankari);

        //ControllerOne.Listen(Button.Back, ButtonState.Pressed, Exit, "Poistu pelistä");
        //ControllerOne.Listen(Button.DPadLeft, ButtonState.Down, Liikuta, "Pelaaja liikkuu vasemmalle", pelaaja1,
        //    -NOPEUS);
        //ControllerOne.Listen(Button.DPadRight, ButtonState.Down, Liikuta, "Pelaaja liikkuu oikealle", pelaaja1, NOPEUS);
        //ControllerOne.Listen(Button.A, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, HYPPYNOPEUS);
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
    }
    
    
    /// <summary>
    /// Aliohjelma, jossa peliin lisätään pelin laskurit.
    /// </summary>
    /// <param name="nimi">Laskurin nimi</param>
    /// <param name="laskuri">Laskuri jonka luvut näytetään</param>
    /// <param name="paikka">Laskurin sijainti näytöllä</param>
    /// <returns></returns>
    private Label LuoNaytto(string nimi, IntMeter laskuri, int paikka)
    {
        Label naytto = new Label(nimi);
        naytto.Title = nimi;
        naytto.X = Screen.Left + 100;
        naytto.Y = Screen.Top - paikka;
        naytto.TextColor = Color.AshGray;
        naytto.Color = Color.Black;
        
        naytto.BindTo(laskuri);
        Add(naytto);

        return naytto;
    }

    
    /*private void LuoSydamet()
    {
        HorizontalLayout asettelu = new HorizontalLayout();
        asettelu.Spacing = 10;

        Widget sydamet = new Widget(asettelu);
        sydamet.Color = Color.Transparent;
        sydamet.X = Screen.Center.X;
        sydamet.Y = Screen.Top - 50;
        Add(sydamet);

        for (int i = 0; i < elamat; i++) 
        {
            Widget sydan = new Widget( 30, 30, Shape.Heart );
            sydan.Color = Color.Red; 
            sydamet.Add(sydan);   
        }
    }*/
    
    
    /// <summary>
    /// Aliohjelma, jota kutsutaan kun painetaan oikeaa tai vasenta nuolta, jolloin sankari liikkuu.
    /// </summary>
    /// <param name="hahmo">Pelin sankari</param>
    /// <param name="nopeus">Liikkumisnopeus</param>
    private void Liikuta(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Walk(nopeus);
    }

    
    /// <summary>
    /// Aliohjelma, jota kutsutaan kun painetaan nuolta ylöspäin, jolloin sankari hyppää.
    /// </summary>
    /// <param name="hahmo">Pelin sankari</param>
    /// <param name="nopeus">Hyppynopeus</param>
    private void Hyppaa(PlatformCharacter hahmo, double nopeus)
    {
        hyppyAani.Play();
        hahmo.Jump(nopeus);
    }
    
    
    /// <summary>
    /// Aliohjelma, jota kutsutaan kun sankari osuu avaruusalukseen.
    /// </summary>
    /// <param name="hahmo">Pelin sankari</param>
    /// <param name="alus">Pelin avaruusalus</param>
    private void TormaaAlukseen(PhysicsObject hahmo, PhysicsObject alus)
    {
        maaliAani.Play();
        MessageDisplay.Add("Pääsit avaruusalukseen!");
        //Voitit(); Tämä oli täällä vanhassa versiossa 261120
        kenttaNro++;
        SeuraavaKentta();
    }

    
    /// <summary>
    /// Aliohjelma, jota kutsutaan kun sankari osuu laavaan.
    /// </summary>
    /// <param name="hahmo">Pelin sankari</param>
    /// <param name="taso">Laavataso</param>
    private void TormaaLaavaan(PhysicsObject hahmo, PhysicsObject taso)
    {
        Kuolit();
    }

    
    /// <summary>
    /// Aliohjelma, jota kutsutaan kun sankari osuu alieniin.
    /// </summary>
    /// <param name="hahmo">Pelin sankari</param>
    /// <param name="alien">Pelin alien eli vihollinen</param>
    private void TormaaAlieniin(PhysicsObject hahmo, PhysicsObject alien)
    {
        kipuAani.Play();
        alien.Destroy();
        elamat.Value--;
        //LuoSydamet(); Yritä saada sydämmet vähenemään tai luo ne uudestaan
        if (elamat <= 0) Kuolit();
    }
    
    
    /// <summary>
    /// Aliohjelma, jota kutsutaan kun painetaan välilyöntiä, jolloin sankari ampuu aseella.
    /// </summary>
    /// <param name="hahmo">Pelin sankari</param>
    private void AmmuAseella(PlatformCharacter hahmo)
    {
        PhysicsObject ammus = sankari.Weapon.Shoot();
        if (ammus != null)
        {
            ammus.Size *= 3;
            ammus.Image = ammusKuva;
        }
    }

    
    /// <summary>
    /// Aliohjelma, jota kutsutaan kun sankarin ampuma ammus osuu alieniin.
    /// </summary>
    /// <param name="ammus">Sankarin ampuma ammus</param>
    /// <param name="kohde">Pelin alien</param>
    private void AmmusOsui(PhysicsObject ammus, PhysicsObject kohde)
    {
        ammus.Destroy();
        if (kohde.Tag.ToString() == "alien")
        {
            alienKuoleeAani.Play();
            kohde.Destroy();
            pisteet.Value++;
            //LuoTimantti();
        }
    }

    
    /// <summary>
    /// Aliohjelma, jota kutsutaan kun sankari kuolee eli menettää kaikki elämäpisteensä.
    /// </summary>
    private void Kuolit()
    {
        //double loppuPisteet = LoppuPisteet();
        topLista.EnterAndShow(pisteet); //loppuPisteet
        topLista.HighScoreWindow.Closed += AloitaPeliHavio;
        kuolemaAani.Play();
        sankari.Destroy();
    }

    
    /// <summary>
    /// Aliohjelma, jota kutsutaan kun sankari voittaa pelin osuessaan avaruusalukseen.
    /// </summary>
    private void Voitit()
    {
        //double loppuPisteet = LoppuPisteet();
        topLista.EnterAndShow(pisteet); //loppuPisteet
        topLista.HighScoreWindow.Closed += AloitaPeliVoitto;
    }

    
    /// <summary>
    /// Aliohjelma, jota kutsutaan kun poistuu top 10 pelaajien valikosta.
    /// </summary>
    /// <param name="sender">Ikkuna, josta poistutaan</param>
    private void AloitaPeliVoitto(Window sender)
    {
        alienLkm.Clear();
        pisteet.Value = 0;
        kenttaNro = 1;
        PeliValikko(this, "PÄÄSIT ALUKSELLE", "PELAA UUDELLEEN", "LOPETA PELI");
    }
    
    
    /// <summary>
    /// Aliohjelma, jota kutsutaan kun poistuu top 10 pelaajien valikosta.
    /// </summary>
    /// <param name="sender">Ikkuna, josta poistutaan</param>
    private void AloitaPeliHavio(Window sender)
    {
        alienLkm.Clear();
        pisteet.Value = 0;
        PeliValikko(this, "KUOLIT", "ALOITA ALUSTA", "LOPETA PELI");
    }
    
    
}