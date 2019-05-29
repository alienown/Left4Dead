using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Left4Dead
{
    //WZORZEC STAN - ZOMBIE W ZALEZNOSCI OD ZYCIA MAJA ROZNE STANY, WPLYWA TO NA ICH PORUSZANIE SIE ORAZ INTERAKCJE Z OTOCZENIEM//
    interface ZombieState
    {
        void findNextStep(Zombie z);
    }
    
    //STAN NORMALNY
    class NormalState : ZombieState
    {
        public void findNextStep(Zombie z)
        {
            Position zombiePositionCopy = new Position(z.zombiePosition);
            Shift shift;

            lock(GameData.consoleAccessObject)
            {
                shift = GameData.gameMechanics.getBestZombieShift(z.zombiePosition);

                switch (GameData.checkForCollision(new Position(z.zombiePosition.x + shift.position.x, z.zombiePosition.y + shift.position.y)))
                {
                    case Collision.NONE:
                        zombiePositionCopy.x += shift.position.x;
                        zombiePositionCopy.y += shift.position.y;
                        break;
                    case Collision.WALL:
                        break;
                    case Collision.ZOMBIE:
                        break;
                    case Collision.DROP:
                        zombiePositionCopy.x += shift.position.x;
                        zombiePositionCopy.y += shift.position.y;
                        GameData.removeDropAtPosition(zombiePositionCopy);
                        break;
                    case Collision.BULLET:
                        break;                  
                }
                z.direction = shift.direction;
                GameData.displayModelAtPosition(zombiePositionCopy, z.zombiePosition, z.zombieModel, z.direction, true);
            }

            if (z.health <= 0.5 * z.maxHealth)
            {
                z.makeUpset();
            }
        }
    }

    //STAN ZDENERWOWANY - CHODZENIE DIAGONALNE, PCHANIE INNYCH ZOMBIE, ULEPSZENIE ATRYBUTOW ZOMBIE
    class AngryState : ZombieState
    {
        public void findNextStep(Zombie z)
        {
            Position zombiePositionCopy = new Position(z.zombiePosition);
            Shift shift;

            lock(GameData.consoleAccessObject)
            {
                shift = GameData.gameMechanics.getBestZombieShift(z.zombiePosition, true);

                switch (GameData.checkForCollision(new Position(z.zombiePosition.x + shift.position.x, z.zombiePosition.y + shift.position.y)))
                {
                    case Collision.NONE:
                        zombiePositionCopy.x += shift.position.x;
                        zombiePositionCopy.y += shift.position.y;
                        break;
                    case Collision.WALL:
                        break;
                    case Collision.ZOMBIE:
                        if(GameData.gameMechanics.tryToPushZombie(new Position(zombiePositionCopy.x + shift.position.x, zombiePositionCopy.y + shift.position.y)))
                        {
                            zombiePositionCopy.x += shift.position.x;
                            zombiePositionCopy.y += shift.position.y;
                        }
                        break;
                    case Collision.DROP:
                        zombiePositionCopy.x += shift.position.x;
                        zombiePositionCopy.y += shift.position.y;
                        GameData.removeDropAtPosition(zombiePositionCopy);
                        break;
                    case Collision.BULLET:
                        break;
                }
                z.direction = shift.direction;
                GameData.displayModelAtPosition(zombiePositionCopy, z.zombiePosition, z.zombieModel, z.direction, true);
            }
        }
    }

    //STAN W SZALE - CHODZENIE DIAGONALNE, ULEPSZENIE ATRYBUTOW, ATAKOWANIE INNYCH ZOMBIE NA SWOJEJ DRODZE
    class EnragedState : ZombieState
    {
        public void findNextStep(Zombie z)
        {
            Position zombiePositionCopy = new Position(z.zombiePosition);
            Shift shift;

            lock(GameData.consoleAccessObject)
            {
                shift = GameData.gameMechanics.getBestZombieShift(z.zombiePosition, true);

                switch (GameData.checkForCollision(new Position(z.zombiePosition.x + shift.position.x, z.zombiePosition.y + shift.position.y)))
                {
                    case Collision.NONE:
                        zombiePositionCopy.x += shift.position.x;
                        zombiePositionCopy.y += shift.position.y;
                        break;
                    case Collision.WALL:
                        break;
                    case Collision.ZOMBIE:
                        lock (GameData.consoleAccessObject)
                        {
                            if (GameData.gameMechanics.hitZombie(new Position(z.zombiePosition.x + shift.position.x, z.zombiePosition.y + shift.position.y), z.attackDamage, false))
                            {
                                zombiePositionCopy.x += shift.position.x;
                                zombiePositionCopy.y += shift.position.y;
                            }
                        }
                        break;
                    case Collision.DROP:
                        zombiePositionCopy.x += shift.position.x;
                        zombiePositionCopy.y += shift.position.y;
                        GameData.removeDropAtPosition(zombiePositionCopy);
                        break;
                    case Collision.BULLET:
                        break;
                }
                z.direction = shift.direction;
                GameData.displayModelAtPosition(zombiePositionCopy, z.zombiePosition, z.zombieModel, z.direction, true);
            }
        }
    }
    /////////////////////////////////////////////////////////////

    //WZORZEC STRATEGIA - ROZNE ALGORYTMY WYBIERANIA ZOMBIE, KTORE MAJA POJAWIC SIE W DANEJ CHWILI NA MAPIE//
    interface Difficulty
    {
        void planNextSpawn(ZombieMaker z);
    }

    //STRATEGIA LATWA - STALY INTERWAL POMIEDZY SPAWNAMI(2,5 SEKUNDY), POJAWIAJA SIE TYLKO NORMALNE ZOMBIE, MAX 10 ZOMBIE
    class EasyDifficulty : Difficulty
    {
        public void planNextSpawn(ZombieMaker z)
        {         
            if (z.zombieCount < 10)
            {
                z.whichZombie = "NormalZombie";
                //2,5 sekundy pomiedzy spawnami
                z.sleepInterval = 5000;
            }
            else
            {
                z.sleepInterval = 0;
                z.whichZombie = "None";
            }         
        }

        public override string ToString()
        {
            return "Easy";
        }
    }

    //STRATEGIA SREDNIA - WYBOR ZOMBIE ZALEZNY OD WYNIKU GRACZA, INTERWAL POMIEDZY SPAWNAMI(1S-2,5S), POJAWIAC SIE MOGA TEZ TANKI, MAX 10 ZOMBIE
    class MediumDifficulty : Difficulty
    {
        public void planNextSpawn(ZombieMaker z)
        {
            if (z.zombieCount < 10)
            {
                //wybor zombie zalezny od wyniku gracza (jezeli gracz osiagnie score rowny 50 - spawnic sie beda same tanki)
                if(z.playerScore - GameData.generateRandomNumber(5,50) < 0)
                {
                    z.whichZombie = "NormalZombie";
                }
                else
                {
                    z.whichZombie = "TankZombie";
                }

                //interwal pomiedzy spawnami - od 1 sekundy do 2,5
                z.sleepInterval = GameData.generateRandomNumber(1000, 2501);
            }
            else
            {
                z.sleepInterval = 0;
                z.whichZombie = "None";
            }
        }

        public override string ToString()
        {
            return "Medium";
        }
    }

    //STRATEGIA TRUDNA - WYBOR ZOMBIE ZALEZNY OD WYNIKU GRACZA ORAZ JEGO ZDROWIA, INTERWAL ZALEZNY OD WYNIKU GRACZA, POJAWIAC SIE MOGA TANKI I HUNTERY, MAX 10 ZOMBIE
    class HardDifficulty : Difficulty
    {
        public void planNextSpawn(ZombieMaker z)
        {
            int interval;

            double scorePerMinute = (z.playerScore * 60) / z.gameTime;

            if (z.zombieCount < 10)
            {
                //zombie tym trudniejszy im wiecej punktow na minute i wiecej zycia ma gracz

                if (scorePerMinute <= 20)
                {
                    z.whichZombie = "NormalZombie";
                }
                else if (scorePerMinute <= GameData.generateRandomNumber(0, z.playerHealth))
                { 
                    z.whichZombie = "TankZombie";
                }
                else
                {
                    z.whichZombie = "HunterZombie";
                }

                //interwal miedzy spawnami tym mniejszy im wiekszy jest wynik gracza
                interval = (GameData.generateRandomNumber(1000, 2501) - z.playerScore * 10);

                if (interval < 0)
                {
                    z.sleepInterval = 0;
                }
                else
                {
                    z.sleepInterval = interval;
                }
            }
            else
            {
                z.sleepInterval = 0;
                z.whichZombie = "None";
            }
        }

        public override string ToString()
        {
            return "Hard";
        }
    }
    /////////////////////////////////////////////////////////////

    //WZORZEC OBSERWATOR - POWIADAMIAMY WATKI O SPAUZOWANIU ROZGRYWKI//
    abstract class Observer
    {
        protected bool isPaused;
        protected Pause pause;

        public abstract void Update();
    }

    //ZAWIERA LISTE OBSERWATOROW, POWIADAMIA ICH O ZMIANIE SWOJEGO STANU
    abstract class Subject
    {
        private List<Observer> observers;

        public Subject()
        {
            observers = new List<Observer>();
        }

        public void Attach(Observer o)
        {
            lock (GameData.isPausedAccessObject)
            {
                observers.Add(o);
            }
        }

        public void Detach(Observer o)
        {
            lock (GameData.isPausedAccessObject)
            {
                observers.Remove(o);
            }
        }

        protected void Notify()
        {
            lock (GameData.isPausedAccessObject)
            {
                foreach (Observer o in observers)
                {
                    o.Update();
                }
            }
        }
    }

    class Pause : Subject
    {
        private bool isPaused;

        public Pause()
        {
            isPaused = false;
        }

        public bool getState()
        {
            lock (GameData.isPausedAccessObject)
            {
                return isPaused;
            }
        }

        public void setState()
        {
            lock (GameData.isPausedAccessObject)
            {
                if (getState() == true)
                {
                    isPaused = false;
                    GameData.pauseGameEvent.Set();
                }
                else
                {
                    isPaused = true;
                }
                Notify();
            }
        }
    }

    //ODPOWIADA ZA ZNIKANIE DROPA PO OKRESLONYM CZASIE, SYGNALIZUJE ZBLIZAJACY SIE KONIEC MIGAWKA
    class DropTracker : Observer
    {
        public DropTracker(Pause pause)
        {
            pause.Attach(this);
            this.pause = pause;
            isPaused = pause.getState();
        }
       
        public void trackDrop(Position position, Drop drop)
        {
            int appearTime = drop.getAppearTime();
            try
            {
                int pom = 1;
                while (appearTime > 0)
                {
                    if (isPaused)
                    {
                        GameData.pauseGameEvent.Reset();
                        GameData.pauseGameEvent.WaitOne();
                    }

                    Thread.Sleep(1000);
                    appearTime -= 1000;
                    if (appearTime <= 4000)
                    {
                        if (pom == 1)
                        {
                            drop.getModel().modelColor = ConsoleColor.White;
                            pom = 0;
                        }
                        else
                        {
                            drop.getModel().modelColor = drop.getModel().modelBaseColor;
                            pom = 1;
                        }
                        GameData.displayModelAtPosition(position, position, drop.getModel(), 0, false);
                    }
                }
            }
            catch (ThreadInterruptedException e)
            {

            }
            GameData.removeDropAtPosition(position);
            pause.Detach(this);
        }

        public override void Update()
        {
            isPaused = pause.getState();
        }
    }

    //KLASA BAZOWA DLA ZOMBIE
    abstract class Zombie : Observer
    {
        //stan zombie(podczas stworzenia zombie ustawiany na normalny)
        public ZombieState zombieState;

        public Zombie(Pause pause)
        {
            pause.Attach(this);
            this.pause = pause;
            isPaused = pause.getState();
        }

        //metody abstrakcyjne wywolywane w metodzie szablonowej
        abstract protected void FollowPlayer();
        abstract protected void dropGoodies();
        abstract protected void addScore();

        //metody rozwscieczajaca danego zombie(przenosi go do innego stanu)
        public abstract void makeUpset();

        //zabicie zombie
        public abstract void kill();

        public Direction direction { get; set; }
        public Thread zombieThread { get; set; }
        private bool drop = true;
        public bool isPushable { get; set; }
        public int health { get; set; }
        public Position zombiePosition { get; set; }

        public int walkSpeed { get; set; }
        public int attackSpeed { get; set; }
        public int attackDamage { get; set; }
        public int maxHealth { get; set; }
        public Model zombieModel { get; set; }

        //ustawia czy zombie moze dropic itemy czy nie
        public void setDrop(bool set)
        {
            drop = set;
        }      

        //sprawdza czy gracz jest w poblizu (1 kratka dookola)
        public bool isPlayerAround()
        {
            lock(GameData.consoleAccessObject)
            {
                if (Math.Abs(zombiePosition.x - GameData.player.position.x) <= 1 && Math.Abs(zombiePosition.y - GameData.player.position.y) <= 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        //przed spawnem szuka pozycji wolnej na mapie do pojawienia sie
        public Position findSpawnPosition()
        {
            Position position;
            Collision collision;

            do
            {
                position = new Position(GameData.generateRandomNumber(19, 82), GameData.generateRandomNumber(6, 44));
                collision = GameData.checkForCollision(position);
            }
            while (collision != Collision.NONE);

            return position;
        }

        //cykl zycia zombie
        public void liveTemplateMethod()
        {
            try
            {
                do
                {
                    if (isPaused)
                    {
                        GameData.pauseGameEvent.Reset();
                        GameData.pauseGameEvent.WaitOne();
                    }
                    FollowPlayer();
                    Thread.Sleep(walkSpeed);
                } while (health > 0);
            }
            catch (ThreadInterruptedException e) { }
            if (drop == true)
            {
                dropGoodies();
            }
            addScore();
            pause.Detach(this);
        }

        public override void Update()
        {
            isPaused = pause.getState();
        }
    }

    //ODPOWIADA ZA CZAS OGLUSZENIA GRACZA PRZEZ TANKA
    class StunTracker : Observer
    {
        public StunTracker(Pause pause)
        {
            pause.Attach(this);
            this.pause = pause;
            isPaused = pause.getState();
        }

        public void trackStun()
        {
            //2 sekundy stuna po kolizji
            int stunTime = 2000;

            try
            {
                while (stunTime > 0)
                {
                    Thread.Sleep(16);
                    stunTime -= 16;
                    lock (GameData.consoleAccessObject)
                    {
                        Console.SetCursorPosition(0, 3);
                        Console.Write("    ");
                        Console.Write(stunTime);
                    }
                    if (isPaused)
                    {
                        GameData.pauseGameEvent.Reset();
                        GameData.pauseGameEvent.WaitOne();
                    }
                }
            }
            catch(ThreadInterruptedException e) { }

            lock (GameData.playerStunnedAccess)
            {
                GameData.player.isStunned = false;
            }
            pause.Detach(this);
            GameData.removeThread(Thread.CurrentThread);
        }

        public override void Update()
        {
            isPaused = pause.getState();
        }
    }

    //ODPOWIADA ZA LOT HUNTERA PODCZAS SKOKU
    class JumpTracker : Observer
    {
        public JumpTracker(Pause pause)
        {
            pause.Attach(this);
            this.pause = pause;
            isPaused = pause.getState();
        }

        public void jump(Direction direction, HunterZombie zombie)
        {
            bool isDone = false;
            int jumpRange = zombie.jumpRange;
            Position zombiePositionCopy = new Position(zombie.zombiePosition);
            int verticalShift = 0, horizontalShift = 0;

            switch (direction)
            {
                case Direction.UP:
                    verticalShift = 0;
                    horizontalShift = -1;
                    break;
                case Direction.RIGHT:
                    verticalShift = 1;
                    horizontalShift = 0;
                    break;
                case Direction.DOWN:
                    verticalShift = 0;
                    horizontalShift = 1;
                    break;
                case Direction.LEFT:
                    verticalShift = -1;
                    horizontalShift = 0;
                    break;
            }

            try
            {
                while (!isDone && jumpRange > 0)
                {
                    if (isPaused)
                    {
                        GameData.pauseGameEvent.Reset();
                        GameData.pauseGameEvent.WaitOne();
                    }

                    lock(GameData.consoleAccessObject)
                    {
                        switch (GameData.checkForCollision(new Position(zombie.zombiePosition.x + verticalShift, zombie.zombiePosition.y + horizontalShift)))
                        {
                            case Collision.NONE:
                                zombiePositionCopy.y += horizontalShift;
                                zombiePositionCopy.x += verticalShift;
                                jumpRange -= 1;
                                break;
                            case Collision.PLAYER:
                                lock (GameData.playerHealthAccess)
                                {
                                    GameData.gameMechanics.hitPlayer(zombie.attackDamage);
                                    GameData.playerHealthStatusEvent.Set();
                                }
                                isDone = true;
                                break;
                            case Collision.WALL:
                                isDone = true;
                                break;
                            case Collision.ZOMBIE:
                                isDone = true;
                                break;
                            case Collision.DROP:
                                zombiePositionCopy.y += horizontalShift;
                                zombiePositionCopy.x += verticalShift;
                                jumpRange -= 1;
                                GameData.removeDropAtPosition(zombiePositionCopy);
                                break;
                        }
                        GameData.displayModelAtPosition(zombiePositionCopy, zombie.zombiePosition, zombie.zombieModel, direction, true);
                    }
                    Thread.Sleep(50);
                }
            }
            catch (ThreadInterruptedException e)
            {

            }
            GameData.jumpWait.Set();
            pause.Detach(this);
        }

        public override void Update()
        {
            isPaused = pause.getState();
        }
    }

    //ODPOWIADA ZA LOT GRACZA PO MAPIE PO UDERZENIU PRZEZ TANKA, PO SKONCZONYM LOCIE OGLUSZA GRACZA
    class PushTracker : Observer
    {
        public PushTracker(Pause pause)
        {
            lock (GameData.playerStunnedAccess)
            {
                GameData.player.isStunned = true;
            }
            pause.Attach(this);
            this.pause = pause;
            isPaused = pause.getState();
        }

        public void pushAndStun(Direction direction)
        {
            Position playerPositionCopy = new Position(GameData.player.position);
            bool isDone = false;

            GameData.player.direction = direction;

            int verticalShift = 0, horizontalShift = 0;

            switch (direction)
            {
                case Direction.UP:
                    verticalShift = 0;
                    horizontalShift = -1;
                    break;
                case Direction.RIGHT:
                    verticalShift = 1;
                    horizontalShift = 0;
                    break;
                case Direction.DOWN:
                    verticalShift = 0;
                    horizontalShift = 1;
                    break;
                case Direction.LEFT:
                    verticalShift = -1;
                    horizontalShift = 0;
                    break;
            }

            try
            {
                while (!isDone)
                {
                    if (isPaused)
                    {
                        GameData.pauseGameEvent.Reset();
                        GameData.pauseGameEvent.WaitOne();
                    }

                    lock (GameData.consoleAccessObject)
                    {
                        switch (GameData.checkForCollision(new Position(GameData.player.position.x + verticalShift, GameData.player.position.y + horizontalShift)))
                        {
                            case Collision.NONE:
                                playerPositionCopy.y += horizontalShift;
                                playerPositionCopy.x += verticalShift;
                                break;
                            case Collision.WALL:
                                lock (GameData.playerHealthAccess)
                                {
                                    if (GameData.player.health > 0)
                                    {
                                        GameData.player.health = GameData.player.health - 5;
                                        GameData.playerHealthStatusEvent.Set();
                                    }
                                }
                                isDone = true;
                                break;
                            case Collision.ZOMBIE:
                                lock (GameData.playerHealthAccess)
                                {
                                    if (GameData.player.health > 0)
                                    {
                                        GameData.player.health = GameData.player.health - 5;
                                        GameData.playerHealthStatusEvent.Set();
                                    }
                                }
                                isDone = true;
                                break;
                            case Collision.DROP:
                                playerPositionCopy.y += horizontalShift;
                                playerPositionCopy.x += verticalShift;
                                GameData.removeDropAtPosition(playerPositionCopy);
                                break;
                        }
                       // if (playerPositionCopy.Equals(GameData.player.position))
                      //  {
                      //      Console.WriteLine("");
                      //  }

                        GameData.displayModelAtPosition(playerPositionCopy, GameData.player.position, GameData.player.playerModel, GameData.player.direction, true);
                    }

                    Thread.Sleep(50);
                }

                Thread stunTrackingThread;
                stunTrackingThread = new Thread(() => new StunTracker(pause).trackStun());
                stunTrackingThread.Start();
                GameData.addThread(stunTrackingThread);
                pause.Detach(this);
            }
            catch(ThreadInterruptedException e) { }
        }

        public override void Update()
        {
            isPaused = pause.getState();
        }
    }

    //ODPOWIADA ZA LOT POCISKU PO STRZALE
    class Bullet : Observer
    {
        public Bullet(Position playerPosition, Direction direction, Model model, int range, Pause pause)
        {
            bulletPosition = new Position(playerPosition);
            this.direction = direction;
            bulletModel = model;
            this.range = range;

            pause.Attach(this);
            this.pause = pause;
            isPaused = pause.getState();
        }

        int range;
        Direction direction;
        Position bulletPosition;
        Model bulletModel;

        bool isDone = false, clearPrevious = false;

        public void Fly()
        {
            int verticalShift = 0, horizontalShift = 0;

            switch (direction)
            {
                case Direction.UP:
                    verticalShift = 0;
                    horizontalShift = -1;
                    break;
                case Direction.RIGHT:
                    verticalShift = 1;
                    horizontalShift = 0;
                    break;
                case Direction.DOWN:
                    verticalShift = 0;
                    horizontalShift = 1;
                    break;
                case Direction.LEFT:
                    verticalShift = -1;
                    horizontalShift = 0;
                    break;
            }

            while (!isDone && range > 0)
            {
                if (isPaused)
                {
                    GameData.pauseGameEvent.Reset();
                    GameData.pauseGameEvent.WaitOne();
                }

                lock (GameData.consoleAccessObject)
                {
                    Position bulletPositionCopy = new Position(bulletPosition);
                    switch (GameData.checkForCollision(new Position(bulletPosition.x + verticalShift, bulletPosition.y + horizontalShift)))
                    {
                        case Collision.NONE:
                            bulletPositionCopy.x += verticalShift;
                            bulletPositionCopy.y += horizontalShift;
                            GameData.displayModelAtPosition(bulletPositionCopy, bulletPosition, bulletModel, direction, clearPrevious);
                            break;
                        case Collision.WALL:
                            if (!GameData.player.position.Equals(bulletPosition))
                                GameData.clearPosition(bulletPosition);
                            isDone = true;
                            break;
                        case Collision.ZOMBIE:
                            bulletPositionCopy.x += verticalShift;
                            bulletPositionCopy.y += horizontalShift;
                            GameData.gameMechanics.hitZombie(bulletPositionCopy, GameData.player.currentWeapon.damage);
                            if (clearPrevious)
                                GameData.clearPosition(bulletPosition);
                            isDone = true;
                            break;
                        case Collision.DROP:
                            bulletPositionCopy.x += verticalShift;
                            bulletPositionCopy.y += horizontalShift;
                            GameData.clearPosition(bulletPosition);
                            GameData.removeDropAtPosition(bulletPositionCopy);
                            isDone = true;
                            break;
                    }

                    if (range == 1)
                    {
                        GameData.clearPosition(bulletPosition);
                    }
                }

                range -= 1;
                clearPrevious = true;
                Thread.Sleep(GameData.player.currentWeapon.bulletSpeed);
            }
            pause.Detach(this);
        }

        public override void Update()
        {
            isPaused = pause.getState();
        }
    }

    //DO WZORCA STRATEGIA - ZOMBIEMAKER JEST TEZ CZESCIA STRATEGII, ODPOWIADA ZA TWORZENIE ZOMBIE - JEST KONTEKSTEM KLAS STRATEGII
    class ZombieMaker : Observer
    {
        private Difficulty difficulty;
        public int zombieCount, sleepInterval, playerHealth, playerScore;
        public double gameTime;
        public string whichZombie;
        Zombie zombie;

        public ZombieMaker(Difficulty difficulty, Pause pause)
        {
            this.difficulty = difficulty;
            pause.Attach(this);
            this.pause = pause;
            isPaused = pause.getState();
            gameTime = 1;
        }

        public void makeZombies()
        {
            try
            {
                while (true)
                {
                    if (isPaused)
                    {
                        GameData.pauseGameEvent.Reset();
                        GameData.pauseGameEvent.WaitOne();
                    }
                    lock (GameData.zombieAtPositionDictionaryAccess)
                    {
                        zombieCount = GameData.zombieAtPosition.Count;
                    }
                    lock (GameData.playerScoreAccessObject)
                    {
                        playerScore = GameData.player.score;
                    }
                    lock(GameData.playerHealthAccess)
                    {
                        playerHealth = GameData.player.health;
                    }

                    difficulty.planNextSpawn(this);

                    switch(whichZombie)
                    {
                        case "NormalZombie":
                            zombie = new NormalZombie(pause);
                            break;
                        case "TankZombie":
                            zombie = new TankZombie(pause);
                            break;
                        case "HunterZombie":
                            zombie = new HunterZombie(pause);
                            break;
                        case "None":
                            break;
                    }
                    gameTime += sleepInterval / 1000;
                    Thread.Sleep(sleepInterval);
                }
            }
            catch (ThreadInterruptedException e) { }
        }

        public override void Update()
        {
            isPaused = pause.getState();
        }
    }

    //KLASA SLEDZACA ATRYBUTY GRACZA
    class PlayerStatusTracker : Observer
    {
        public PlayerStatusTracker(Pause pause)
        {
            pause.Attach(this);
            this.pause = pause;
            this.isPaused = pause.getState();
        }

        //sledzi zycie gracza, odpowiada rowniez za uruchomienie funkcji wyswietlajacej interfejs podczas smierci postaci(gdy wykryje ze mamy 0 punktow zdrowia)
        public void trackPlayerHealthStatus()
        {
            try
            {
                lock (GameData.consoleAccessObject)
                {
                    Console.SetCursorPosition(105, 6);
                    lock (GameData.playerHealthAccess)
                    {
                        Console.Write("Health: ");
                        Console.ForegroundColor = GameData.gameMechanics.getFontColourBasedOnHealth(GameData.player.health, GameData.player.maxHealth, GameData.playerHealthAccess);
                        Console.Write(GameData.player.health);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                while (true)
                {
                    if (isPaused)
                    {
                        GameData.pauseGameEvent.Reset();
                        GameData.pauseGameEvent.WaitOne();
                    }
                    GameData.playerHealthStatusEvent.Reset();
                    GameData.playerHealthStatusEvent.WaitOne();
                    lock (GameData.consoleAccessObject)
                    {
                        GameData.player.playerModel.modelColor = GameData.gameMechanics.getFontColourBasedOnHealth(GameData.player.health, GameData.player.maxHealth, GameData.playerHealthAccess);
                        GameData.displayModelAtPosition(GameData.player.position, GameData.player.position, GameData.player.playerModel, GameData.player.direction, true);
                        Console.SetCursorPosition(113, 6);
                        lock (GameData.playerHealthAccess)
                        {
                            Console.Write("   ");
                            Console.ForegroundColor = GameData.gameMechanics.getFontColourBasedOnHealth(GameData.player.health, GameData.player.maxHealth, GameData.playerHealthAccess);
                            Console.SetCursorPosition(113, 6);
                            Console.Write(GameData.player.health);
                            Console.ForegroundColor = ConsoleColor.White;

                            if (GameData.player.health <= 0)
                            {
                                Thread endingThread = new Thread(() => GameData.gameMechanics.gameOver());
                                endingThread.Start();
                                break;
                            }
                        }
                    }
                }
            }
            catch (ThreadInterruptedException e) { }
        }

        public void trackPlayerAmmoStatus()
        {
            try
            {
                lock (GameData.consoleAccessObject)
                {
                    Console.SetCursorPosition(105, 14);
                    lock (GameData.weaponAccessObject)
                    {
                        Console.Write("Ammo: " + GameData.player.currentWeapon.showCurrentAmmo());
                    }
                }
                while (true)
                {
                    if (isPaused)
                    {
                        GameData.pauseGameEvent.Reset();
                        GameData.pauseGameEvent.WaitOne();
                    }
                    GameData.playerAmmoStatusEvent.Reset();
                    GameData.playerAmmoStatusEvent.WaitOne();
                    lock (GameData.consoleAccessObject)
                    {
                        Console.SetCursorPosition(111, 14);
                        lock (GameData.weaponAccessObject)
                        {
                            Console.Write("         ");
                            Console.SetCursorPosition(111, 14);
                            Console.Write(GameData.player.currentWeapon.showCurrentAmmo());
                        }
                    }
                }
            }
            catch (ThreadInterruptedException e) { }
        }

        public void trackPlayerCurrentWeaponStatus()
        {
            try
            {
                lock (GameData.consoleAccessObject)
                {
                    Console.SetCursorPosition(105, 10);
                    lock (GameData.weaponAccessObject)
                    {
                        Console.Write("Weapon: " + GameData.player.currentWeapon.ToString());
                    }
                }
                while (true)
                {
                    if (isPaused)
                    {
                        GameData.pauseGameEvent.Reset();
                        GameData.pauseGameEvent.WaitOne();
                    }
                    GameData.playerCurrentWeaponStatusEvent.Reset();
                    GameData.playerCurrentWeaponStatusEvent.WaitOne();
                    lock (GameData.consoleAccessObject)
                    {
                        Console.SetCursorPosition(113, 10);
                        lock (GameData.weaponAccessObject)
                        {
                            Console.Write("            ");
                            Console.SetCursorPosition(113, 10);
                            Console.Write(GameData.player.currentWeapon.ToString());
                        }
                    }
                }
            }
            catch (ThreadInterruptedException e) { }
        }

        public void trackPlayerScoreStatus()
        {
            try
            {
                lock (GameData.consoleAccessObject)
                {
                    Console.SetCursorPosition(105, 18);
                    lock (GameData.weaponAccessObject)
                    {
                        Console.Write("Score: " + GameData.player.score);
                    }
                }
                while (true)
                {
                    if (isPaused)
                    {
                        GameData.pauseGameEvent.Reset();
                        GameData.pauseGameEvent.WaitOne();
                    }
                    GameData.playerScoreStatusEvent.Reset();
                    GameData.playerScoreStatusEvent.WaitOne();
                    lock (GameData.consoleAccessObject)
                    {
                        Console.SetCursorPosition(112, 18);
                        lock (GameData.weaponAccessObject)
                        {
                            Console.Write("            ");
                            Console.SetCursorPosition(112, 18);
                            Console.Write(GameData.player.score);
                        }
                    }
                }
            }
            catch (ThreadInterruptedException e) { }
        }

        public override void Update()
        {
            isPaused = pause.getState();
        }
    }
    /////////////////////////////////////////////////////////////

    //////WZORZEC SINGLETON - JEDEN GRACZ NA CALA ROZGRYWKE/////
    //KLASA PLAYER - GLOBALNE DANE O GRACZU, JEGO ATRYBUTY//
    class Player : IDisposable
    {
        private Player()
        {
            Name = GameData.playerName;
            playerModel = new PlayerModel(GameData.playerModel);
            score = 0;
            position = new Position(65, 25);
            health = 100;
            direction = Direction.DOWN;
            weapons = new List<Weapon>() { new Pistol() };
            currentWeapon = weapons.ElementAt(0);
        }

        private static Player player = null;
        public static Player getPlayer()
        {
            if (player == null)
            {
                lock (GameData.playerObjectAccess)
                {
                    if (player == null)
                    {
                        player = new Player();
                    }
                }
            }
            return player;
        }

        public void Dispose()
        {
            player = null;
        }

        public Model playerModel;
        public Direction direction;
        public Position position;
        public int health;
        public int maxHealth = 100;
        public List<Weapon> weapons;
        public Weapon currentWeapon;
        public bool isStunned = false;
        public bool isReloaded = true;
        public int score = 0;
        public string Name;
    }

    //WZORZEC BUILDER - GRAFICZNY INTERFEJS ROZGRYWKI//
    interface Builder
    {
        void setBackgroundColor();
        void setUpperLeftCorner();
        void setUpperSide();
        void setUpperRightCorner();
        void setRightSide();
        void setLowerRightCorner();
        void setLowerSide();
        void setLowerLeftCorner();
        void setLeftSide();
        Product getProduct();
    }

    //DOMYSLNY WYGLAD INTERFEJSU ROZGRYWKI - CZARNY BACKGROUND, RAMKA ZLOZONA Z POPRZECZEK, ASCII
    class DefaultInterfaceBuilder : Builder
    {
        private Product gameInterface;

        public DefaultInterfaceBuilder()
        {
            gameInterface = new Product();
        }

        public Product getProduct()
        {
            return gameInterface;
        }

        public void setBackgroundColor()
        {
            gameInterface.backgroundColor = ConsoleColor.Black;
        }

        public void setLeftSide()
        {
            for (int i = 0; i <= 38; i++)
                gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '|', Encoding.ASCII));
        }

        public void setLowerLeftCorner()
        {
            gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '<', Encoding.ASCII));
        }

        public void setLowerRightCorner()
        {
            gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '.', Encoding.ASCII));
        }

        public void setLowerSide()
        {
            for (int i = 0; i <= 82; i++)
                gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '=', Encoding.ASCII));
        }

        public void setRightSide()
        {
            for (int i = 0; i <= 38; i++)
                gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '|', Encoding.ASCII));
        }

        public void setUpperLeftCorner()
        {
            gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '.', Encoding.ASCII));
        }

        public void setUpperRightCorner()
        {
            gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '.', Encoding.ASCII));
        }

        public void setUpperSide()
        {
            for (int i = 0; i <= 82; i++)
                gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '=', Encoding.ASCII));
        }
    }

    //WYGLAD MATEMATYCZNY - SZARY BACKGROUND, RAMKA ZLOZONA Z FIGUR GEOMETRYCZNYCH I CYFR, UNICODE
    class MathemathicsInterfaceBuilder : Builder
    {
        private Product gameInterface;

        public MathemathicsInterfaceBuilder()
        {
            gameInterface = new Product();
        }

        public Product getProduct()
        {
            return gameInterface;
        }

        public void setBackgroundColor()
        {
            gameInterface.backgroundColor = ConsoleColor.DarkGray;
        }

        public void setLeftSide()
        {
            int j = 0;
            for (int i = 0; i <= 38; i++)
            {
                if (j == 0)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25F6', Encoding.UTF8));
                }
                else if (j == 1)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25FB', Encoding.UTF8));
                }
                else if (j == 2)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25EF', Encoding.UTF8));
                }
                else if (j == 3)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25AD', Encoding.UTF8));
                }
                else if (j == 4)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25B3', Encoding.UTF8));
                }
                else if (j == 5)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25D8', Encoding.UTF8));
                    j = 0;
                }
                j++;
            }
        }

        public void setLowerLeftCorner()
        {
            gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25FA', Encoding.UTF8));
        }

        public void setLowerRightCorner()
        {
            gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25FF', Encoding.UTF8));
        }

        public void setLowerSide()
        {
            int j = 49;
            for (int i = 0; i <= 82; i++)
            {
                if (j == 49)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                }
                else if (j == 50)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                }
                else if (j == 51)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                }
                else if (j == 52)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                }
                else if (j == 53)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                }
                else if (j == 54)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                    j = 49;
                }
                j++;
            }
        }

        public void setRightSide()
        {
            int j = 0;
            for (int i = 0; i <= 38; i++)
            {
                if (j == 0)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25F6', Encoding.UTF8));
                }
                else if (j == 1)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25FB', Encoding.UTF8));
                }
                else if (j == 2)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25EF', Encoding.UTF8));
                }
                else if (j == 3)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25AD', Encoding.UTF8));
                }
                else if (j == 4)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25B3', Encoding.UTF8));
                }
                else if (j == 5)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25D8', Encoding.UTF8));
                    j = 0;
                }
                j++;
            }
        }

        public void setUpperLeftCorner()
        {
            gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25F8', Encoding.UTF8));
        }

        public void setUpperRightCorner()
        {
            gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, '\u25F9', Encoding.UTF8));
        }

        public void setUpperSide()
        {
            int j = 49;
            for (int i = 0; i <= 82; i++)
            {
                if (j == 49)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                }
                else if (j == 50)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                }
                else if (j == 51)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                }
                else if (j == 52)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                }
                else if (j == 53)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                }
                else if (j == 54)
                {
                    gameInterface.frameCharacters.Add(new FrameCharacter(ConsoleColor.White, (char)j, Encoding.UTF8));
                    j = 49;
                }
                j++;
            }

        }
    }

    //WYGLAD TECZOWY - NIEBIESKI BACKGROUND, TECZOWA RAMKA ZLOZONA Z POPRZECZEK, UNICODE
    class RainbowInterfaceBuilder : Builder
    {
        private Product gameInterface;

        private ConsoleColor[] rainbowColors = { ConsoleColor.Red, ConsoleColor.DarkYellow, ConsoleColor.Yellow, ConsoleColor.Green, ConsoleColor.Cyan, ConsoleColor.Magenta };
        private int nextColor = 0;

        public RainbowInterfaceBuilder()
        {
            gameInterface = new Product();
        }

        public Product getProduct()
        {
            return gameInterface;
        }

        public void setBackgroundColor()
        {
            gameInterface.backgroundColor = ConsoleColor.Blue;
        }

        public void setLeftSide()
        {
            for (int i = 0; i <= 38; i++)
            {
                gameInterface.frameCharacters.Add(new FrameCharacter(rainbowColors[nextColor % 5], '║', Encoding.UTF8));
                nextColor++;
            }
        }

        public void setLowerLeftCorner()
        {
            gameInterface.frameCharacters.Add(new FrameCharacter(rainbowColors[nextColor % 5], '╚', Encoding.UTF8));
            nextColor++;
        }

        public void setLowerRightCorner()
        {
            gameInterface.frameCharacters.Add(new FrameCharacter(rainbowColors[nextColor % 5], '╝', Encoding.UTF8));
            nextColor++;
        }

        public void setLowerSide()
        {
            for (int i = 0; i <= 82; i++)
            {
                gameInterface.frameCharacters.Add(new FrameCharacter(rainbowColors[nextColor % 5], '═', Encoding.UTF8));
                nextColor++;
            }
        }

        public void setRightSide()
        {
            for (int i = 0; i <= 38; i++)
            {
                gameInterface.frameCharacters.Add(new FrameCharacter(rainbowColors[nextColor % 5], '║', Encoding.UTF8));
                nextColor++;
            }
        }

        public void setUpperLeftCorner()
        {
            gameInterface.frameCharacters.Add(new FrameCharacter(rainbowColors[nextColor % 5], '╔', Encoding.UTF8));
            nextColor++;
        }

        public void setUpperRightCorner()
        {
            gameInterface.frameCharacters.Add(new FrameCharacter(rainbowColors[nextColor % 5], '╗', Encoding.UTF8));
            nextColor++;
        }

        public void setUpperSide()
        {
            for (int i = 0; i <= 82; i++)
            {
                gameInterface.frameCharacters.Add(new FrameCharacter(rainbowColors[nextColor % 5], '═', Encoding.UTF8));
                nextColor++;
            }
        }
    }

    //PRODUKTEM STWORZONYM PRZEZ BUILDERA JEST OBIEKT SKLADAJACY SIE Z KOLORU TLA, ORAZ LISTY ZNAKOW, GDZIE KAZDY Z NICH MOZE BYC ZAPISANY W INNYM FORMACIE ORAZ INNYM KOLORZE
    class Product
    {
        public Product()
        {
            frameCharacters = new List<FrameCharacter>();
        }

        public ConsoleColor backgroundColor { get; set; }
        public List<FrameCharacter> frameCharacters { get; set; }
    }

    //ZNAK BEDACY CZESCIA RAMKI
    class FrameCharacter
    {
        public FrameCharacter(ConsoleColor color, char character, Encoding encoding)
        {
            this.character = character;
            characterColor = color;
            this.encoding = encoding;
        }

        public ConsoleColor characterColor { get; set; }
        public char character { get; set; }
        public Encoding encoding { get; set; }
    }

    //DIRECTOR - WYDAJE POLECENIA KONSTRUKCJI INTERFEJSU ROZGRYWKI ODPOWIEDNIEMU BUILDEROWI
    class InterfaceDirector
    {
        Builder builder;

        public InterfaceDirector(Builder builder)
        {
            this.builder = builder;
        }

        public void Construct()
        {
            builder.setBackgroundColor();
            builder.setUpperSide();
            builder.setUpperRightCorner();
            builder.setRightSide();
            builder.setLowerRightCorner();
            builder.setLowerSide();
            builder.setLowerLeftCorner();
            builder.setLeftSide();
            builder.setUpperLeftCorner();
        }
    }
    //////////////////////////////////////////////////////////

    //WZORZEC DEKORATOR - ZOMBIE UPUSZCZAJA ROZNE PRZEDMIOTY//
    abstract class Drop
    {
        //dodaje wartosc zwiazana z przedmiotem dla gracza
        public abstract void addValue();
        public abstract Thread getTrackingThread();
        public abstract void setTrackingThread(Thread trackingThread);
        public abstract int getAppearTime();
        //zwraca model zwiazany z danym przedmiotem
        public abstract Model getModel();
    }

    //BONUS ZWIAZANY Z PRZEDMIOTEM
    class Bonus : Drop
    {
        protected Drop drop;

        public Bonus(Drop drop)
        {
            this.drop = drop;
        }

        public override void addValue()
        {
            drop.addValue();
        }

        public override int getAppearTime()
        {
            return drop.getAppearTime();
        }

        public override Model getModel()
        {
            return drop.getModel();
        }

        public override Thread getTrackingThread()
        {
            return drop.getTrackingThread();
        }

        public override void setTrackingThread(Thread trackingThread)
        {
            drop.setTrackingThread(trackingThread);
        }
    }

    //BONUS PUNKTOW ZDROWIA
    class HealthBonus : Bonus
    {
        public HealthBonus(Drop drop) : base(drop)
        {
        }

        public override Thread getTrackingThread()
        {
            return base.getTrackingThread();
        }

        public override void setTrackingThread(Thread trackingThread)
        {
            base.setTrackingThread(trackingThread);
        }

        public override int getAppearTime()
        {
            return base.getAppearTime();
        }

        //jezeli przedmiot zawiera bonus zdrowia, background przedmiotu bedzie zielony
        public override Model getModel()
        {
            Model model = base.getModel();
            model.backgroundColor = ConsoleColor.Green;
            return model;
        }

        //bonus zdrowia dodaje 50 punktow zdrowia
        public override void addValue()
        {
            lock (GameData.playerHealthAccess)
            {
                if (GameData.player.health + 50 <= 100)
                {
                    GameData.player.health += 50;
                }
                else
                {
                    GameData.player.health = 100;
                }
                base.addValue();
                GameData.playerHealthStatusEvent.Set();
            }
        }
    }

    //BONUS AMUNICJI
    class AmmoBonus : Bonus
    {
        public AmmoBonus(Drop drop) : base(drop)
        {
        }

        public override Thread getTrackingThread()
        {
            return base.getTrackingThread();
        }

        public override void setTrackingThread(Thread trackingThread)
        {
            base.setTrackingThread(trackingThread);
        }

        public override int getAppearTime()
        {
            return base.getAppearTime();
        }

        //jezeli przedmiot zawiera bonus amunicji, foreground kolor bedzie w tym przypadku zolty
        public override Model getModel()
        {
            Model model = base.getModel();
            model.modelColor = ConsoleColor.Yellow;
            return model;
        }

        //dodaje 3 naboje do ak47, 1 naboj do snajperki(dziala w zaleznosci od zalozonej broni)
        public override void addValue()
        {
            lock (GameData.weaponAccessObject)
            {
                if (GameData.player.currentWeapon.ToString() == "Pistol")
                {
                    if (GameData.player.weapons.Exists(w => w.ToString() == "AK47"))
                    {
                        Weapon ak47 = GameData.player.weapons.Find(w => w.ToString() == "AK47");
                        if (ak47.ammo + 3 <= ak47.maxAmmo)
                        {
                            ak47.ammo += 3;
                        }
                        else
                        {
                            ak47.ammo = ak47.maxAmmo;
                        }
                    }
                    else if (GameData.player.weapons.Exists(w => w.ToString() == "Sniper Rifle"))
                    {
                        Weapon sniperRifle = GameData.player.weapons.Find(w => w.ToString() == "Sniper Rifle");
                        if (sniperRifle.ammo + 1 <= sniperRifle.maxAmmo)
                        {
                            sniperRifle.ammo += 1;
                        }
                        else
                        {
                            sniperRifle.ammo = sniperRifle.maxAmmo;
                        }
                    }
                }
                else if (GameData.player.currentWeapon.ToString() == "AK47")
                {
                    if (GameData.player.currentWeapon.ammo + 3 <= GameData.player.currentWeapon.maxAmmo)
                    {
                        GameData.player.currentWeapon.ammo += 3;
                    }
                    else
                    {
                        GameData.player.currentWeapon.ammo = GameData.player.currentWeapon.maxAmmo;
                    }
                }
                else if (GameData.player.currentWeapon.ToString() == "Sniper Rifle")
                {
                    if (GameData.player.currentWeapon.ammo + 1 <= GameData.player.currentWeapon.maxAmmo)
                    {
                        GameData.player.currentWeapon.ammo += 1;
                    }
                    else
                    {
                        GameData.player.currentWeapon.ammo = GameData.player.currentWeapon.maxAmmo;
                    }
                }
                GameData.playerAmmoStatusEvent.Set();
            }
            base.addValue();
        }
    }

    //PRZEDMIOTY BEZ BONUSOW MAJA CZERWONY FOREGROUND
    //PACZKA ZDROWIA
    class HealthPack : Drop
    {
        private int appearTime { get; set; }
        private Model model { get; set; }
        private Thread trackingThread { get; set; }

        public HealthPack()
        {
            appearTime = GameData.dropAppearTime;
            model = new HealthPackModel(GameData.healthPackModel);
        }

        public override void setTrackingThread(Thread trackingThread)
        {
            this.trackingThread = trackingThread;
        }

        public override int getAppearTime()
        {
            return appearTime;
        }

        public override Model getModel()
        {
            return model;
        }

        public override Thread getTrackingThread()
        {
            return trackingThread;
        }

        //dodaje 20 punktow zdrowia dla gracza
        public override void addValue()
        {
            lock (GameData.playerHealthAccess)
            {
                if (GameData.player.health + 20 <= 100)
                {
                    GameData.player.health += 20;
                }
                else
                {
                    GameData.player.health = 100;
                }
                GameData.playerHealthStatusEvent.Set();
            }
        }
    }

    //PACZKA Z AMUNICJA
    class AmmoPack : Drop
    {
        private int appearTime { get; set; }
        private Model model { get; set; }
        private Thread trackingThread { get; set; }

        public AmmoPack()
        {
            appearTime = GameData.dropAppearTime;
            model = new AmmoPackModel(GameData.ammoPackModel);
        }

        public override void setTrackingThread(Thread trackingThread)
        {
            this.trackingThread = trackingThread;
        }

        public override int getAppearTime()
        {
            return appearTime;
        }

        public override Model getModel()
        {
            return model;
        }

        public override Thread getTrackingThread()
        {
            return trackingThread;
        }

        //dodaje 5 naboji do ak47, 3 naboje do snajperki(w zaleznosci od wybranej broni)
        public override void addValue()
        {
            lock (GameData.weaponAccessObject)
            {
                if (GameData.player.currentWeapon.ToString() == "Pistol")
                {
                    if (GameData.player.weapons.Exists(w => w.ToString() == "AK47"))
                    {
                        Weapon ak47 = GameData.player.weapons.Find(w => w.ToString() == "AK47");
                        if (ak47.ammo + 5 <= ak47.maxAmmo)
                        {
                            ak47.ammo += 5;
                        }
                        else
                        {
                            ak47.ammo = ak47.maxAmmo;
                        }
                    }
                    else if (GameData.player.weapons.Exists(w => w.ToString() == "Sniper Rifle"))
                    {
                        Weapon sniperRifle = GameData.player.weapons.Find(w => w.ToString() == "Sniper Rifle");
                        if (sniperRifle.ammo + 3 <= sniperRifle.maxAmmo)
                        {
                            sniperRifle.ammo += 3;
                        }
                        else
                        {
                            sniperRifle.ammo = sniperRifle.maxAmmo;
                        }
                    }
                }
                else if (GameData.player.currentWeapon.ToString() == "AK47")
                {
                    if (GameData.player.currentWeapon.ammo + 5 <= GameData.player.currentWeapon.maxAmmo)
                    {
                        GameData.player.currentWeapon.ammo += 5;
                    }
                    else
                    {
                        GameData.player.currentWeapon.ammo = GameData.player.currentWeapon.maxAmmo;
                    }
                }
                else if (GameData.player.currentWeapon.ToString() == "Sniper Rifle")
                {
                    if (GameData.player.currentWeapon.ammo + 3 <= GameData.player.currentWeapon.maxAmmo)
                    {
                        GameData.player.currentWeapon.ammo += 3;
                    }
                    else
                    {
                        GameData.player.currentWeapon.ammo = GameData.player.currentWeapon.maxAmmo;
                    }
                }
                GameData.playerAmmoStatusEvent.Set();
            }
        }
    }

    //AK47
    class AK47Drop : Drop
    {
        private int appearTime { get; set; }
        private Model model { get; set; }
        private Thread trackingThread { get; set; }
    
        public AK47Drop()
        {
            appearTime = GameData.dropAppearTime;
            this.model = new AK47Model(GameData.ak47Model);
        }

        public override void setTrackingThread(Thread trackingThread)
        {
            this.trackingThread = trackingThread;
        }

        public override int getAppearTime()
        {
            return appearTime;
        }

        public override Model getModel()
        {
            return model;
        }

        public override Thread getTrackingThread()
        {
            return trackingThread;
        }

        public override void addValue()
        {
            lock (GameData.weaponAccessObject)
            {
                    if (GameData.player.weapons.Exists(w => w.ToString() == "AK47"))
                    {
                        GameData.player.currentWeapon = GameData.player.weapons.Find(w => w.ToString() == "AK47");
                        GameData.player.currentWeapon.ammo = GameData.player.currentWeapon.maxAmmo;
                    }
                    else
                    {
                        GameData.player.weapons.Add(new AK47());
                        GameData.player.currentWeapon = GameData.player.weapons.Find(w => w.ToString() == "AK47");
                    }

                GameData.playerAmmoStatusEvent.Set();
                GameData.playerCurrentWeaponStatusEvent.Set();
            }
        }
    }

    //SNAJPERKA
    class SniperRifleDrop : Drop
    {
        private int appearTime { get; set; }
        private Model model { get; set; }
        private Thread trackingThread { get; set; }

        public SniperRifleDrop()
        {
            appearTime = GameData.dropAppearTime;
            this.model = new SniperRifleModel(GameData.sniperRifleModel);
        }

        public override void setTrackingThread(Thread trackingThread)
        {
            this.trackingThread = trackingThread;
        }

        public override int getAppearTime()
        {
            return appearTime;
        }

        public override Model getModel()
        {
            return model;
        }

        public override Thread getTrackingThread()
        {
            return trackingThread;
        }

        public override void addValue()
        {
            lock (GameData.weaponAccessObject)
            {
                    if (GameData.player.weapons.Exists(w => w.ToString() == "Sniper Rifle"))
                    {
                        GameData.player.currentWeapon = GameData.player.weapons.Find(w => w.ToString() == "Sniper Rifle");
                        GameData.player.currentWeapon.ammo = GameData.player.currentWeapon.maxAmmo;
                    }
                    else
                    {
                        GameData.player.weapons.Add(new SniperRifle());
                        GameData.player.currentWeapon = GameData.player.weapons.Find(w => w.ToString() == "Sniper Rifle");
                    }
 
                GameData.playerAmmoStatusEvent.Set();
                GameData.playerCurrentWeaponStatusEvent.Set();
            }
        }
    }
    //////////////////////////////////////////////////////////

    /////////////////MODELE WYSTEPUJACE W GRZE/////////////////////
    abstract class Model
    {
        //UP[0] RIGHT[1] DOWN[2] LEFT[3]
        public Model(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor)
        {
            this.model = model;
            this.modelBaseColor = modelBaseColor;
            this.modelColor = modelBaseColor;
            this.backgroundColor = backgroundColor;
        }

        public Model(Model copy)
        {
            model = new char[4] { copy.model[0], copy.model[1], copy.model[2], copy.model[3] };
            modelBaseColor = copy.modelBaseColor;
            modelColor = copy.modelBaseColor;
            backgroundColor = copy.backgroundColor;
        }

        protected char[] model;
        public ConsoleColor modelBaseColor;
        public ConsoleColor modelColor;
        public ConsoleColor backgroundColor;
        public abstract char DisplayModel(int rotation);
        public override bool Equals(object obj)
        {
            foreach(char c in model)
            {
                if((char)obj == c)
                {
                    return true;
                }
            }
            return false;
        }
    }

    class PlayerModel : Model
    {
        public PlayerModel(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }
        public PlayerModel(Model copy) : base(copy) { }
        public override char DisplayModel(int rotation) { return model[rotation]; }
        public override string ToString()
        {
            return "Player model";
        }
    }

    class PistolBulletModel : Model
    {
        public PistolBulletModel(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }
        public PistolBulletModel(Model copy) : base(copy) { }
        public override char DisplayModel(int rotation) { return model[rotation]; }
        public override string ToString()
        {
            return "Pistol bullet model";
        }
    }

    class AK47BulletModel : Model
    {
        public AK47BulletModel(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }
        public AK47BulletModel(Model copy) : base(copy) { }
        public override char DisplayModel(int rotation) { return model[rotation]; }
        public override string ToString()
        {
            return "AK47 bullet model";
        }
    }

    class SniperRifleBulletModel : Model
    {
        public SniperRifleBulletModel(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }
        public SniperRifleBulletModel(Model copy) : base(copy) { }
        public override char DisplayModel(int rotation) { return model[rotation]; }
        public override string ToString()
        {
            return "Sniper rifle bullet model";
        }
    }

    class NormalZombieModel : Model
    {
        public NormalZombieModel(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }
        public NormalZombieModel(Model copy) : base(copy) { }
        public override char DisplayModel(int rotation) { return model[rotation]; }
        public override string ToString()
        {
            return "Normal zombie model";
        }
    }

    class TankZombieModel : Model
    {
        public TankZombieModel(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }
        public TankZombieModel(Model copy) : base(copy) { }
        public override char DisplayModel(int rotation) { return model[rotation]; }
        public override string ToString()
        {
            return "Tank zombie model";
        }
    }

    class HunterZombieModel : Model
    {
        public HunterZombieModel(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }
        public HunterZombieModel(Model copy) : base(copy) { }
        public override char DisplayModel(int rotation) { return model[rotation]; }
        public override string ToString()
        {
            return "Hunter zombie model";
        }
    }

    class AmmoPackModel : Model
    {
        public AmmoPackModel(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }
        public AmmoPackModel(Model copy) : base(copy) { }
        public override char DisplayModel(int rotation) { return model[rotation]; }
        public override string ToString()
        {
            return "Ammo pack model";
        }
    }

    class HealthPackModel : Model
    {
        public HealthPackModel(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }
        public HealthPackModel(Model copy) : base(copy) { }
        public override char DisplayModel(int rotation) { return model[rotation]; }
        public override string ToString()
        {
            return "Health pack model";
        }
    }

    class AK47Model : Model
    {
        public AK47Model(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }
        public AK47Model(Model copy) : base(copy) { }
        public override char DisplayModel(int rotation) { return model[rotation]; }
        public override string ToString()
        {
            return "AK47 weapon model";
        }
    }

    class SniperRifleModel : Model
    {
        public SniperRifleModel(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }
        public SniperRifleModel(Model copy) : base(copy) { }
        public override char DisplayModel(int rotation) { return model[rotation]; }
        public override string ToString()
        {
            return "Sniper rifle weapon model";
        }
    }
    /////////////////////////////////////////////////////////////

    ////////POZYCJA POSZCZEGOLNYCH OBIEKTOW NA MAPIE/////////////
    class Position
    {
        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Position(Position position)
        {
            x = position.x;
            y = position.y;
        }

        public int x, y;

        public bool Equals(Position a)
        {
            if (a.x == x && a.y == y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    //KIERUNKI PORUSZANIE SIE - DOSTEPNE GORA, PRAWO, DOL, LEWO//
    public enum Direction
    {
        UP = 0,
        RIGHT = 1,
        DOWN = 2,
        LEFT = 3
    }

    ////////////MOZLIWE DO NAPOTKANIA W GRZE KOLIZJE/////////////
    public enum Collision
    {
        NONE = 0,
        PLAYER = 1,
        ZOMBIE = 2,
        DROP = 3,
        BULLET = 4,
        WALL = 5
    }

    //PRZESUNIECIE - KOLEJNA POZYCJA(WYKORZYSTYWANE W ALGORYTMACH CHODZENIA ZOMBIE)
    class Shift
    {
        public Direction direction;
        public Position position;
    }

    //GLOBALNE DANE GRY DOSTEPNE DLA WATKOW I INNE FUNKCJONALNOSCI - POSIADA FUNKCJE SETGRAPHICS POBIERAJACA PRODUKT Z BUILDERA//
    static class GameData
    {
        //difficulty
        public static string difficulty = "";

        //wspolny input dla funkcji Mechanics.start() i Mechanics.gameOver()
        public static ConsoleKeyInfo keyInfo;

        private static Pause pause;

        public static Mechanics gameMechanics;

        //gracz
        public static Player player;

        //info o watkach
        public static Dictionary<Position, Zombie> zombieAtPosition;
        public static Dictionary<Position, Drop> dropAtPosition;
        private static List<Thread> threadList;

        //zapisuje widok z konsoli
        private static char[,] gameMap;

        //builder
        public static string interfaceBuilder = "";

        //backgroundcolor
        public static ConsoleColor backgroundColor;

        //mutexy
        public static Object jumpThreadAccess = new Object();
        public static Object playerScoreAccessObject = new Object();
        public static Object isPausedAccessObject = new Object();
        public static Object consoleAccessObject = new Object();
        public static Object dropAtPositionDictionaryAccess = new Object();
        public static Object playerHealthAccess = new Object();
        public static Object zombieAtPositionDictionaryAccess = new Object();
        public static Object reloadAccessObject = new Object();
        public static Object weaponAccessObject = new Object();
        public static Object generateRandomNumberAccess = new Object();
        public static Object zombieHealthAccess = new Object();
        public static Object playerStunnedAccess = new Object();
        public static Object playerObjectAccess = new Object();
        public static Object threadListAccess = new Object();

        //mechanizm sygnalizacji watkow
        public static ManualResetEvent reloadEvent;
        public static ManualResetEvent exitGame;
        public static ManualResetEvent playerScoreStatusEvent;
        public static ManualResetEvent keyInfoWaitEvent;
        public static ManualResetEvent playerHealthStatusEvent;
        public static ManualResetEvent playerAmmoStatusEvent;
        public static ManualResetEvent playerCurrentWeaponStatusEvent;
        public static ManualResetEvent pauseGameEvent;
        public static ManualResetEvent jumpWait;

        //modele wystepujace w grze
        public static PlayerModel playerModel;
        public static HealthPackModel healthPackModel;
        public static AmmoPackModel ammoPackModel;
        public static AK47Model ak47Model;
        public static PistolBulletModel pistolBulletModel;
        public static AK47BulletModel aK47BulletModel;
        public static SniperRifleBulletModel sniperRifleBulletModel;
        public static SniperRifleModel sniperRifleModel;
        public static NormalZombieModel normalZombieModel;
        public static TankZombieModel tankZombieModel;
        public static HunterZombieModel hunterZombieModel;

        //10 sekund dla dropow na znikniecie
        public static int dropAppearTime = 10000;

        //wybrana opcja po kliknieciu enter (w dowolnym menu)
        public static string option;

        public static string playerName;     

        //czysci znaki z prostokata wyznaczonego przez pozycje
        public static void clearText(Position from, Position to)
        {
            for(int j = from.y; j < to.y; j++)
            {
                for (int i = from.x; i < to.x; i++)
                {
                    Console.SetCursorPosition(i, j);
                    Console.Write(' ');
                }
            }
        }

        //rysuje napis left4head w menu
        public static void drawText()
        {
            string line;
            try
            {
                int y = 3;
                StreamReader file = new StreamReader(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\l4d.txt");

                while ((line = file.ReadLine()) != null)
                {
                    if (y >= 3 && y < 7)
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    else if (y == 7)
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                    else
                        Console.ForegroundColor = ConsoleColor.Red;
                    Console.SetCursorPosition(30, y++);
                    Console.WriteLine(line);
                }
                file.Close();
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (FileNotFoundException e) { }
        }

        //metoda inicjalizujaca stan gry przed jej rozpoczeciem
        public static void initializeGame()
        {
            Console.Clear();

            Difficulty difficulty;

            if (GameData.difficulty == "Hard")
            {
                difficulty = new HardDifficulty();
            }
            else if (GameData.difficulty == "Medium")
            {
                difficulty = new MediumDifficulty();
            }
            else
            {
                GameData.difficulty = "Easy";
                difficulty = new EasyDifficulty();
            }

            Builder interfaceBuilder;

            if (GameData.interfaceBuilder == "Mathemathics")
            {
                interfaceBuilder = new MathemathicsInterfaceBuilder();
            }
            else if (GameData.interfaceBuilder == "Rainbow")
            {
                interfaceBuilder = new RainbowInterfaceBuilder();
            }
            else
            {
                interfaceBuilder = new DefaultInterfaceBuilder();
            }

            GameData.pause = new Pause();

            GameData.gameMechanics = new Mechanics(GameData.pause);

            PlayerStatusTracker playerStatusTracker = new PlayerStatusTracker(GameData.pause);

            //pole po ktorym mozna sie poruszac + brak podlogi w konsoli
            GameData.setGraphics(interfaceBuilder);
            Console.SetCursorPosition(105, 22);
            Console.Write("Difficulty: " + difficulty.ToString());
            Console.CursorVisible = false;

            GameData.gameMap = new char[50, 130];
            for (int i = 0; i < 50; i++)
            {
                for (int j = 0; j < 130; j++)
                {
                    GameData.gameMap[i, j] = ' ';
                }
            }

            //inicjalizacja modeli
            GameData.tankZombieModel = new TankZombieModel(new char[4] { '¥', '¥', '¥', '¥' }, ConsoleColor.Green, GameData.backgroundColor);
            GameData.normalZombieModel = new NormalZombieModel(new char[4] { '@', '@', '@', '@' }, ConsoleColor.Green, GameData.backgroundColor);
            GameData.hunterZombieModel = new HunterZombieModel(new char[4] { 'x', 'x', 'x', 'x' }, ConsoleColor.Green, GameData.backgroundColor);
            GameData.healthPackModel = new HealthPackModel(new char[4] { '+', '+', '+', '+' }, ConsoleColor.Red, GameData.backgroundColor);
            GameData.ammoPackModel = new AmmoPackModel(new char[4] { '#', '#', '#', '#' }, ConsoleColor.Red, GameData.backgroundColor);
            GameData.ak47Model = new AK47Model(new char[4] { 'A', 'A', 'A', 'A' }, ConsoleColor.Red, GameData.backgroundColor);
            GameData.sniperRifleModel = new SniperRifleModel(new char[4] { 'S', 'S', 'S', 'S' }, ConsoleColor.Red, GameData.backgroundColor);
            GameData.pistolBulletModel = new PistolBulletModel(new char[4] { '.', '.', '.', '.' }, ConsoleColor.White, GameData.backgroundColor);
            GameData.aK47BulletModel = new AK47BulletModel(new char[4] { '°', '°', '°', '°' }, ConsoleColor.White, GameData.backgroundColor);
            GameData.sniperRifleBulletModel = new SniperRifleBulletModel(new char[4] { '│', '─', '│', '─' }, ConsoleColor.White, GameData.backgroundColor);
            GameData.playerModel = new PlayerModel(new char[4] { '^', '>', 'v', '<' }, ConsoleColor.Green, GameData.backgroundColor);

            GameData.zombieAtPosition = new Dictionary<Position, Zombie>();
            GameData.dropAtPosition = new Dictionary<Position, Drop>();

            //gracz
            GameData.player = Player.getPlayer();

            player.Name = GameData.playerName;
            player.playerModel = new PlayerModel(new char[4] { '^', '>', 'v', '<' }, ConsoleColor.Green, GameData.backgroundColor);
            player.score = 0;
            player.position = new Position(65, 25);
            player.health = 100;
            player.direction = Direction.DOWN;
            player.weapons = new List<Weapon>() { new Pistol() };
            player.currentWeapon = player.weapons.ElementAt(0);
            GameData.displayModelAtPosition(player.position, player.position, player.playerModel, Direction.DOWN, true);

            //inicjalizacja eventow (mechanizmy sygnalizacji watkow)
            GameData.reloadEvent = new ManualResetEvent(false);
            GameData.jumpWait = new ManualResetEvent(false);
            GameData.pauseGameEvent = new ManualResetEvent(false);
            GameData.playerHealthStatusEvent = new ManualResetEvent(false);
            GameData.keyInfoWaitEvent = new ManualResetEvent(false);
            GameData.playerAmmoStatusEvent = new ManualResetEvent(false);
            GameData.playerCurrentWeaponStatusEvent = new ManualResetEvent(false);
            GameData.playerScoreStatusEvent = new ManualResetEvent(false);

            player.isReloaded = true;
            GameData.threadList = new List<Thread>();

            Thread playerScoreStatusThread = new Thread(() => playerStatusTracker.trackPlayerScoreStatus());
            Thread playerCurrentWeaponStatusThread = new Thread(() => playerStatusTracker.trackPlayerCurrentWeaponStatus());
            Thread playerAmmoStatusThread = new Thread(() => playerStatusTracker.trackPlayerAmmoStatus());
            Thread playerHealthStatusThread = new Thread(() => playerStatusTracker.trackPlayerHealthStatus());
            Thread reloadWeaponThread = new Thread(() => gameMechanics.reloadWeapon());
            Thread zombieMaker = new Thread(() => new ZombieMaker(difficulty, GameData.pause).makeZombies());
            Thread start = new Thread(() => GameData.gameMechanics.startGame());

            GameData.addThread(playerScoreStatusThread);
            GameData.addThread(playerCurrentWeaponStatusThread);
            GameData.addThread(playerAmmoStatusThread);
            GameData.addThread(playerHealthStatusThread);
            GameData.addThread(reloadWeaponThread);
            GameData.addThread(zombieMaker);

            playerScoreStatusThread.Start();
            playerCurrentWeaponStatusThread.Start();
            playerAmmoStatusThread.Start();
            playerHealthStatusThread.Start();
            reloadWeaponThread.Start();
            zombieMaker.Start();
            start.Start();
        }

        public static void addThread(Thread thread)
        {
            lock(GameData.threadListAccess)
            {
                GameData.threadList.Add(thread);
            }
        }

        public static void removeThread(Thread thread)
        {
            lock (GameData.threadListAccess)
            {
                GameData.threadList.Remove(thread);
            }
        }

        //usuwa pozostale po grze watki (wiem, ze GC, ale przezorny zawsze ubezpieczony)
        public static void removeAllThreads()
        {
            lock(GameData.consoleAccessObject)
            {
                foreach (Zombie zombie in zombieAtPosition.Values)
                {
                    zombie.setDrop(false);
                }
                foreach (Thread t in threadList)
                {
                    t.Interrupt();
                }
            }
        }

        public static void setGraphics(Builder interfaceBuilder)
        {
            int j = 0;

            InterfaceDirector director = new InterfaceDirector(interfaceBuilder);
            director.Construct();

            Product frame = interfaceBuilder.getProduct();

            lock (GameData.consoleAccessObject)
            {
                Console.BackgroundColor = frame.backgroundColor;
                GameData.backgroundColor = frame.backgroundColor;

                Console.Clear();
               
                //pozioma belka góra             
                for (int i = 0; i <= 82; i++)
                {
                    Console.ForegroundColor = frame.frameCharacters[j].characterColor;
                    Console.SetCursorPosition(19 + i, 5);
                    Console.OutputEncoding = frame.frameCharacters[j].encoding;
                    Console.Write(frame.frameCharacters[j].character);
                    j++;
                }
                //prawo-gora róg
                Console.ForegroundColor = frame.frameCharacters[j].characterColor;
                Console.SetCursorPosition(102, 5);
                Console.OutputEncoding = frame.frameCharacters[j].encoding;
                Console.Write(frame.frameCharacters[j].character);
                j++;
                //pionowa belka prawo
                for (int i = 0; i <= 38; i++)
                {
                    Console.ForegroundColor = frame.frameCharacters[j].characterColor;
                    Console.SetCursorPosition(102, 6 + i);
                    Console.OutputEncoding = frame.frameCharacters[j].encoding;
                    Console.Write(frame.frameCharacters[j].character);
                    j++;
                }
                //prawo-dol róg
                Console.ForegroundColor = frame.frameCharacters[j].characterColor;
                Console.SetCursorPosition(102, 45);
                Console.OutputEncoding = frame.frameCharacters[j].encoding;
                Console.Write(frame.frameCharacters[j].character);
                j++;
                //pozioma belka dol
                for (int i = 0; i <= 82; i++)
                {
                    Console.ForegroundColor = frame.frameCharacters[j].characterColor;
                    Console.SetCursorPosition(101 - i, 45);
                    Console.OutputEncoding = frame.frameCharacters[j].encoding;
                    Console.Write(frame.frameCharacters[j].character);
                    j++;
                }
                //lewo-dol róg
                Console.ForegroundColor = frame.frameCharacters[j].characterColor;
                Console.SetCursorPosition(18, 45);
                Console.OutputEncoding = frame.frameCharacters[j].encoding;
                Console.Write(frame.frameCharacters[j].character);
                j++;
                //pionowa belka lewo
                for (int i = 0; i <= 38; i++)
                {
                    Console.ForegroundColor = frame.frameCharacters[j].characterColor;
                    Console.SetCursorPosition(18, 44 - i);
                    Console.OutputEncoding = frame.frameCharacters[j].encoding;
                    Console.Write(frame.frameCharacters[j].character);
                    j++;
                }
                Console.SetCursorPosition(18, 5);
                //lewo-gora róg
                Console.ForegroundColor = frame.frameCharacters[j].characterColor;
                Console.OutputEncoding = frame.frameCharacters[j].encoding;
                Console.Write(frame.frameCharacters[j].character);
            }

            Console.OutputEncoding = Encoding.UTF8;
        }

        //weryfikuje czy pozycja nie jest czescia ramki (ogranicza ruch zombie i gracza)
        public static bool validatePosition(Position position)
        {
            if (position.x >= 19 && position.x <= 101)
            {
                if (position.y >= 6 && position.y <= 44)
                {
                    return true;
                }
            }
            return false;
        }

        //sprawdza jaki obiekt znajduje sie na konkretnej pozycji w macierzy (rownowazne z konsola)
        public static Collision checkForCollision(Position position)
        {
           // ICollection<Position> keys = GameData.zombieAtPosition.Keys;
            //Position position2 = GameData.player.position;

            if (validatePosition(position) == true)
            {
                lock (consoleAccessObject)
                {
                    if (gameMap[position.y, position.x] != ' ')
                    {
                        if (normalZombieModel.Equals(gameMap[position.y, position.x]) || hunterZombieModel.Equals(gameMap[position.y, position.x])
                            || tankZombieModel.Equals(gameMap[position.y, position.x]))
                        {
                            return Collision.ZOMBIE;
                        }
                        else if (GameData.playerModel.Equals(gameMap[position.y, position.x]))
                        {
                            return Collision.PLAYER;
                        }
                        else if (player.currentWeapon.bulletModel.Equals(gameMap[position.y, position.x]))
                        {
                            return Collision.BULLET;
                        }
                        else
                        {
                            return Collision.DROP;
                        }
                    }
                    else
                    {
                        return Collision.NONE;
                    }
                }
            }
            else
            {
                return Collision.WALL;
            }
        }

        public static void addZombieAtPosition(Position position, Zombie zombie)
        {
            lock (consoleAccessObject)
            {
                lock (zombieAtPositionDictionaryAccess)
                {
                    zombieAtPosition.Add(position, zombie);
                }
                displayModelAtPosition(position, position, zombie.zombieModel, Direction.DOWN, true);
            }
        }

        public static void removeZombieAtPosition(Position position)
        {
            lock (consoleAccessObject)
            {
                lock (zombieAtPositionDictionaryAccess)
                {
                    Zombie zombie = zombieAtPosition[position];

                    lock (jumpThreadAccess)
                    {
                        zombie.kill();
                    }
                }
            }
        }

        public static void addDropAtPosition(Position position, Drop drop)
        {
            lock (consoleAccessObject)
            {
                lock (dropAtPositionDictionaryAccess)
                {
                    dropAtPosition.Add(position, drop);
                }
                displayModelAtPosition(position, position, drop.getModel(), Direction.DOWN, false);
            }
        }

        //usuwamy drop z gameMapa, konsoli i ze slownika, wylaczamy przy tym wyjatek od trackowania dropa
        public static void removeDropAtPosition(Position position)
        {
            lock (dropAtPositionDictionaryAccess)
            {
                Position _position = dropAtPosition.Keys.Where(p => p.Equals(position)).FirstOrDefault();

                if (_position != null)
                {
                    dropAtPosition[_position].getTrackingThread().Interrupt();
                    dropAtPosition.Remove(_position);
                    clearPosition(_position);
                }
            }
        }

        //generuje liczbe pseudolosowa z okreslonego przedzialu
        public static int generateRandomNumber(int lower = 0, int upper = 0)
        {
            lock (generateRandomNumberAccess)
            {
                return new Random().Next(lower, upper);
            }
        }

        //wyswietla okreslony model w konsoli i wpisuje go do macierzy, poprzednia pozycja sluzy do opcjonalnego jej czyszczenia, ustawiamy tu rowniez rotacje modelu i kolor jego rysowania
        public static void displayModelAtPosition(Position nextPosition, Position position, Model model, Direction dir, bool clearPrevious = true)
        {
            lock (consoleAccessObject)
            {
                Console.ForegroundColor = model.modelColor;
                Console.BackgroundColor = model.backgroundColor;

                if (!position.Equals(nextPosition))
                {
                    if (clearPrevious)
                        clearPosition(position);

                    position.x = nextPosition.x;
                    position.y = nextPosition.y;

                    Console.SetCursorPosition(position.x, position.y);
                    gameMap[position.y, position.x] = model.DisplayModel((int)dir);
                    Console.Write(model.DisplayModel((int)dir));
                }
                else
                {
                    gameMap[position.y, position.x] = model.DisplayModel((int)dir);
                    Console.SetCursorPosition(position.x, position.y);
                    Console.Write(model.DisplayModel((int)dir));
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = GameData.backgroundColor;
            }
        }

        //czysci okreslona pozycje w macierzy i w konsoli
        static public void clearPosition(Position position)
        {
            lock (consoleAccessObject)
            {
                Console.SetCursorPosition(position.x, position.y);
                gameMap[position.y, position.x] = ' ';
                Console.Write(" ");
            }
        }
    }

    //MECHANIKA GRY, PORUSZANIE SIE, STRZELANIE, PAUZOWANIE, ITP. JEST TO CZESC OBSERWERA!//
    class Mechanics : Observer
    {
        public Mechanics(Pause pause)
        {
            pause.Attach(this);
            this.pause = pause;
            isPaused = pause.getState();
        }

        public bool tryToPushZombie(Position position)
        {
            Zombie zombie;

            lock (GameData.zombieAtPositionDictionaryAccess)
            {
                position = GameData.zombieAtPosition.Keys.Where(p => p.Equals(position)).FirstOrDefault();
                zombie = GameData.zombieAtPosition[position];
            }

            int verticalShift = GameData.generateRandomNumber(-1, 2);
            int horizontalShift = GameData.generateRandomNumber(-1, 2);

            Position nextPosition = new Position(position.x + verticalShift, position.y + horizontalShift);

            if (GameData.checkForCollision(nextPosition) == (int)Collision.NONE)
            {
                if(zombie.isPushable)
                {
                    GameData.displayModelAtPosition(nextPosition, position, zombie.zombieModel, zombie.direction);
                    return true;
                }
            }

            return false;
        }

        //zwraca przesuniecie dla kolejnego kroku zombie, ustalany jest na podstawie pozycji gracza i pozycji zombie
        public Shift getBestZombieShift(Position zombiePosition, bool possibleDiagonal = false)
        {
            Shift shift = new Shift();
            Position playerPosition = GameData.player.position;

            //tylko na krzyz
            if (possibleDiagonal == false)
            {
                if (Math.Abs(playerPosition.x - zombiePosition.x) > Math.Abs(playerPosition.y - zombiePosition.y))
                {
                    if (playerPosition.x - zombiePosition.x > 0)
                    {
                        //prawo
                        shift.position = new Position(1, 0);
                        shift.direction = Direction.RIGHT;
                    }
                    else
                    {
                        //lewo
                        shift.position = new Position(-1, 0);
                        shift.direction = Direction.LEFT;
                    }
                }
                else
                {
                    if (playerPosition.y - zombiePosition.y > 0)
                    {
                        //dol
                        shift.position = new Position(0, 1);
                        shift.direction = Direction.DOWN;
                    }
                    else
                    {
                        //gora
                        shift.position = new Position(0, -1);
                        shift.direction = Direction.UP;
                    }
                }
            }
            else //mozliwe chodzenie po skosie
            {
                if (playerPosition.x == zombiePosition.x)
                {
                    if (playerPosition.y - zombiePosition.y > 0)
                    {
                        //na dole od nas
                        shift.position = new Position(0, 1);
                        shift.direction = Direction.DOWN;
                    }
                    else
                    {
                        //na gorze od nas
                        shift.position = new Position(0, -1);
                        shift.direction = Direction.UP;
                    }
                }
                else if (playerPosition.y == zombiePosition.y)
                {
                    if (playerPosition.x - zombiePosition.x > 0)
                    {
                        // na prawo od nas
                        shift.position = new Position(1, 0);
                        shift.direction = Direction.RIGHT;
                    }
                    else
                    {
                        // na lewo od nas
                        shift.position = new Position(-1, 0);
                        shift.direction = Direction.LEFT;
                    }
                }
                else
                {
                    if (playerPosition.x > zombiePosition.x && playerPosition.y < zombiePosition.y)
                    {
                        //gora prawo
                        shift.position = new Position(1, -1);
                        shift.direction = Direction.UP;
                    }
                    else if (playerPosition.x < zombiePosition.x && playerPosition.y < zombiePosition.y)
                    {
                        //gora lewo
                        shift.position = new Position(-1, -1);
                        shift.direction = Direction.UP;
                    }
                    else if (playerPosition.x > zombiePosition.x && playerPosition.y > zombiePosition.y)
                    {
                        //dol prawo
                        shift.position = new Position(1, 1);
                        shift.direction = Direction.DOWN;
                    }
                    else if (playerPosition.x < zombiePosition.x && playerPosition.y > zombiePosition.y)
                    {
                        //dol lewo
                        shift.position = new Position(-1, 1);
                        shift.direction = Direction.DOWN;
                    }
                }
            }

            return shift;
        }

        //ustawia kolor modelu w zaleznosci od liczy punktow zycia
        public ConsoleColor getFontColourBasedOnHealth(int health, int maxHealth, Object lockObject)
        {
            lock (lockObject)
            {
                if (health >= 0.75 * maxHealth)
                {
                    return ConsoleColor.Green;
                }
                else if (health >= 0.5 * maxHealth)
                {
                    return ConsoleColor.Yellow;
                }
                else if (health >= 0.25 * maxHealth)
                {
                    return ConsoleColor.DarkYellow;
                }
                else
                {
                    return ConsoleColor.Red;
                }
            }
        }

        //odpowiada za czas przeladowania broni
        public void reloadWeapon()
        {
            int reloadSpeed;

            try
            {
                while (true)
                {
                    GameData.reloadEvent.Reset();
                    GameData.reloadEvent.WaitOne();

                    lock (GameData.weaponAccessObject)
                    {
                        reloadSpeed = GameData.player.currentWeapon.reloadSpeed;
                    }

                    
                    while (reloadSpeed > 0)
                    {
                        Thread.Sleep(16);
                        reloadSpeed -= 16;
                        lock (GameData.consoleAccessObject)
                        {
                            Console.SetCursorPosition(0, 0);
                            Console.Write("    ");
                            Console.Write(reloadSpeed);
                        }
                        if (isPaused)
                        {
                            GameData.pauseGameEvent.Reset();
                            GameData.pauseGameEvent.WaitOne();
                        }
                    }
                    
                    lock (GameData.weaponAccessObject)
                    {
                        GameData.player.isReloaded = true;
                    }
                }
            }
            catch (ThreadInterruptedException e) { }
        }

        //metoda odpowiadajaca ze akcje postaci
        private void performAction(ConsoleKeyInfo keyInfo)
        {
            Position playerPositionCopy;
            bool wait = false;

            lock (GameData.playerStunnedAccess)
            {
                if (GameData.player.isStunned)
                    wait = true;
            }

            if (wait == true)
            {
            }
            else if (isPaused)
            {
                GameData.pauseGameEvent.Reset();
                GameData.pauseGameEvent.WaitOne();
            }
            else
            {
                playerPositionCopy = new Position(GameData.player.position);

                lock (GameData.consoleAccessObject)
                {
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.RightArrow:
                            switch (GameData.checkForCollision(new Position(GameData.player.position.x + 1, GameData.player.position.y)))
                            {
                                case Collision.NONE:
                                    playerPositionCopy.x++;
                                    break;
                                case Collision.WALL:
                                    break;
                                case Collision.ZOMBIE:
                                    break;
                                case Collision.DROP:
                                    playerPositionCopy.x++;
                                    grabDrop(playerPositionCopy);
                                    GameData.removeDropAtPosition(playerPositionCopy);
                                    break;
                            }
                            GameData.player.direction = Direction.RIGHT;
                            GameData.displayModelAtPosition(playerPositionCopy, GameData.player.position, GameData.player.playerModel, Direction.RIGHT, true);
                            break;

                        case ConsoleKey.LeftArrow:
                            switch (GameData.checkForCollision(new Position(GameData.player.position.x - 1, GameData.player.position.y)))
                            {
                                case Collision.NONE:
                                    playerPositionCopy.x--;
                                    break;
                                case Collision.WALL:
                                    break;
                                case Collision.ZOMBIE:
                                    break;
                                case Collision.DROP:
                                    playerPositionCopy.x--;
                                    grabDrop(playerPositionCopy);
                                    GameData.removeDropAtPosition(playerPositionCopy);
                                    break;
                            }
                            GameData.player.direction = Direction.LEFT;
                            GameData.displayModelAtPosition(playerPositionCopy, GameData.player.position, GameData.player.playerModel, Direction.LEFT, true);
                            break;

                        case ConsoleKey.UpArrow:
                            switch (GameData.checkForCollision(new Position(GameData.player.position.x, GameData.player.position.y - 1)))
                            {
                                case Collision.NONE:
                                    playerPositionCopy.y--;
                                    break;
                                case Collision.WALL:
                                    break;
                                case Collision.ZOMBIE:
                                    break;
                                case Collision.DROP:
                                    playerPositionCopy.y--;
                                    grabDrop(playerPositionCopy);
                                    GameData.removeDropAtPosition(playerPositionCopy);
                                    break;
                            }
                            GameData.player.direction = Direction.UP;
                            GameData.displayModelAtPosition(playerPositionCopy, GameData.player.position, GameData.player.playerModel, Direction.UP, true);
                            break;

                        case ConsoleKey.DownArrow:
                            switch (GameData.checkForCollision(new Position(GameData.player.position.x, GameData.player.position.y + 1)))
                            {
                                case Collision.NONE:
                                    playerPositionCopy.y++;
                                    break;
                                case Collision.WALL:
                                    break;
                                case Collision.ZOMBIE:
                                    break;
                                case Collision.DROP:
                                    playerPositionCopy.y++;
                                    grabDrop(playerPositionCopy);
                                    GameData.removeDropAtPosition(playerPositionCopy);
                                    break;
                            }
                            GameData.player.direction = Direction.DOWN;
                            GameData.displayModelAtPosition(playerPositionCopy, GameData.player.position, GameData.player.playerModel, Direction.DOWN, true);
                            break;

                        case ConsoleKey.Spacebar:
                            lock (GameData.weaponAccessObject)
                            {
                                if (GameData.player.currentWeapon.ammo == -1)
                                {
                                    if (GameData.player.isReloaded == true)
                                    {
                                        Thread InstanceCaller = new Thread(() => shoot(GameData.player.currentWeapon.bulletModel, GameData.player.currentWeapon.range));
                                        GameData.addThread(InstanceCaller);
                                        InstanceCaller.Start();
                                        GameData.player.isReloaded = false;
                                        GameData.reloadEvent.Set();
                                    }
                                }
                                else if (GameData.player.currentWeapon.ammo > 0)
                                {
                                    if (GameData.player.isReloaded == true)
                                    {
                                        Thread InstanceCaller = new Thread(() => shoot(GameData.player.currentWeapon.bulletModel, GameData.player.currentWeapon.range));
                                        GameData.addThread(InstanceCaller);
                                        InstanceCaller.Start();
                                        GameData.player.currentWeapon.ammo -= 1;
                                        GameData.player.isReloaded = false;
                                        GameData.playerAmmoStatusEvent.Set();
                                        GameData.reloadEvent.Set();
                                    }
                                }
                            }
                            break;
                        case ConsoleKey.D1:
                            lock (GameData.weaponAccessObject)
                            {
                                Weapon weapon = GameData.player.weapons.Find(w => w.ToString() == "Pistol");
                                if (!(weapon == null) && GameData.player.currentWeapon != weapon)
                                {
                                    GameData.player.currentWeapon = weapon;
                                    GameData.playerAmmoStatusEvent.Set();
                                    GameData.playerCurrentWeaponStatusEvent.Set();
                                }
                            }
                            break;
                        case ConsoleKey.D2:
                            lock (GameData.weaponAccessObject)
                            {
                                Weapon weapon = GameData.player.weapons.Find(w => w.ToString() == "AK47");
                                if (!(weapon == null) && GameData.player.currentWeapon != weapon)
                                {
                                    GameData.player.currentWeapon = weapon;
                                    GameData.playerAmmoStatusEvent.Set();
                                    GameData.playerCurrentWeaponStatusEvent.Set();
                                }
                            }
                            break;
                        case ConsoleKey.D3:
                            lock (GameData.weaponAccessObject)
                            {
                                Weapon weapon = GameData.player.weapons.Find(w => w.ToString() == "Sniper Rifle");
                                if (!(weapon == null) && GameData.player.currentWeapon != weapon)
                                {
                                    GameData.player.currentWeapon = weapon;
                                    GameData.playerAmmoStatusEvent.Set();
                                    GameData.playerCurrentWeaponStatusEvent.Set();
                                }
                            }
                            break;
                    }
                }
            }
        }

        //zadaje obrazenia dla zombiaka(ta funkcja wywoluje od razu display, zeby zaktualizowac model zombiaka w miare jego malejacego zycia)
        public bool hitZombie(Position position, int damage, bool drops = true)
        {
            Zombie zombie;
            lock (GameData.consoleAccessObject)
            {
                lock(GameData.zombieAtPositionDictionaryAccess)
                {
                    position = GameData.zombieAtPosition.Keys.Where(p => p.Equals(position)).FirstOrDefault();
                    zombie = GameData.zombieAtPosition[position];
                }
                lock (GameData.zombieHealthAccess)
                {
                    zombie.health -= damage;
                    zombie.zombieModel.modelColor = getFontColourBasedOnHealth(zombie.health, zombie.maxHealth, GameData.zombieHealthAccess);
                    GameData.displayModelAtPosition(position, position, zombie.zombieModel, zombie.direction);
                    if (zombie.health <= 0)
                    {
                        zombie.setDrop(drops);
                        GameData.removeZombieAtPosition(position);
                        return true;
                    }
                }
                return false;
            }
        }

        //zombie bija gracza
        public void hitPlayer(int damage)
        {
            lock (GameData.playerHealthAccess)
            {
                if (GameData.player.health - damage >= 0)
                {
                    GameData.player.health -= damage;
                    GameData.playerHealthStatusEvent.Set();
                }
                else
                {
                    GameData.player.health = 0;
                }
            }
        }

        //zbieramy przedmioty
        private void grabDrop(Position position)
        {
            Drop drop;
            drop = GameData.dropAtPosition[GameData.dropAtPosition.Keys.Where(d => d.Equals(position)).First()];
            drop.addValue();
        }

        //tworzy kule i sprawia ze leca
        private void shoot(Model model, int range)
        {
            try
            {
                Bullet bullet = new Bullet(GameData.player.position, GameData.player.direction, model, range, pause);
                bullet.Fly();
            }
            catch (ThreadInterruptedException e) { GameData.removeThread(Thread.CurrentThread); }
        }

        //koniec gry - interfejs po smierci postaci
        public void gameOver()
        {
            pause.setState();
            Menus.gameOverMenu();
            if (GameData.option == "TRY AGAIN")
            {
                GameData.removeAllThreads();
                GameData.player.Dispose();
                GameData.initializeGame();
            }
            else if (GameData.option == "EXIT TO MAIN MENU")
            {
                exitGame();
            }
        }

        //pauzujemy gre (escape)
        public void pauseGame()
        {
            pause.setState();
            Menus.pauseMenu();
        }

        //wznawiamy rozgyrwke po pauze (klikniecie escape)
        public void resumeGame()
        {
            pause.setState();
        }

        //wychodzimy z rogrywki do menu glownego
        public static void exitGame()
        {
            Console.BackgroundColor = GameData.backgroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            lock (GameData.consoleAccessObject)
            {
                if(GameData.player.health <= 0)
                {
                    Console.SetCursorPosition(53, 27);
                }
                else
                {
                    Console.SetCursorPosition(105, 30);
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Quitting to Main Menu...");
            }
            Mechanics.saveScore();
            GameData.removeAllThreads();
            Thread.Sleep(1000);
            GameData.player.Dispose();
            GameData.exitGame.Set();
        }

        //zapisuje score aktualnej gry
        public static void saveScore()
        {
            string line;
            List<string> words = new List<string>();

            try
            {
                StreamReader file = new StreamReader(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\ranking.txt");
                while ((line = file.ReadLine()) != null)
                {
                    words.Add(line);
                
                }
                words.Add(GameData.player.Name + " " + GameData.player.score + " " + GameData.difficulty);
                file.Close();
            }
            catch (FileNotFoundException e)
            { 
                words.Add(GameData.player.Name + " " + GameData.player.score + " " + GameData.difficulty);
            }

            StreamWriter writer = new StreamWriter(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\ranking.txt", false);

            foreach (var word in words)
            {
                writer.WriteLine(word);
            }
            writer.Close();
        }

        //start rozgrywki
        public void startGame()
        {
            bool dead = false;

            try
            {
                while (true)
                {
                    //czyszcze bufor wejsciowy konsoli - w przypadku stunow gdy klikam klawe, bufor sie zapelnia i po minieciu stuna postac sie teleportuje...
                    while (Console.KeyAvailable)
                    {
                        Console.ReadKey(true);
                    }
                    GameData.keyInfo = Console.ReadKey(true);
                    lock (GameData.playerHealthAccess)
                    {
                        if (GameData.player.health <= 0)
                        {
                            dead = true;
                        }
                    }
                    if (dead == true)
                    {
                        //powiadamiamy interfejs z metody gameOver o zatwierdzeniu operacji (bufor wejsciowy kosnoli jest wspolny dla watkow,
                        //chcialem ominac problem polegajacy na tym, ze kiedy gra sie konczy i pojawia sie menu z gameOver, trzeba kliknac raz jakikolwiek przycisk, zeby uaktywnilo sie menu z gameOver,
                        //wyzej mamy readkey i w metodzie start on nadal czeka na to klikniecie mimo tego, ze juz jestesmy martwi i watek ten powinien zniknac...
                        GameData.keyInfoWaitEvent.Set();
                        if (GameData.keyInfo.Key == ConsoleKey.Enter)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (GameData.keyInfo.Key == ConsoleKey.Escape)
                        {
                            pauseGame();
                            if (GameData.option == "RESUME")
                            {
                                Console.SetCursorPosition(105, 26);
                                Console.Write("                        ");
                                Console.SetCursorPosition(105, 28);
                                Console.Write("                        ");
                                resumeGame();
                            }
                            else if (GameData.option == "EXIT TO MAIN MENU")
                            {
                                exitGame();
                                break;
                            }
                        }
                        else
                        {


                            performAction(GameData.keyInfo);
                        }
                    }
                }
            }
            catch (ThreadInterruptedException e) { }
        }

        public override void Update()
        {
            isPaused = pause.getState();
        }
    }

    //ARSENAL//
    abstract class Weapon
    {
        public int reloadSpeed { get; set; }
        public int bulletSpeed { get; set; }
        public int ammo { get; set; }
        public int damage { get; set; }
        public int range { get; set; }
        public int maxAmmo { get; set; }
        public Model bulletModel { get; set; }

        public abstract string showCurrentAmmo();
    }

    class AK47 : Weapon
    {
        public AK47()
        {
            ammo = 15;
            maxAmmo = 15;
            reloadSpeed = 500;
            bulletSpeed = 10;
            damage = 10;
            range = 40;
            bulletModel = new AK47BulletModel(GameData.aK47BulletModel);
        }

        public override string ToString()
        {
            return "AK47";
        }

        public override string showCurrentAmmo()
        {
            lock(GameData.weaponAccessObject)
            {
                return GameData.player.currentWeapon.ammo.ToString();
            }
        }
    }

    class SniperRifle : Weapon
    {
        public SniperRifle()
        {
            ammo = 5;
            maxAmmo = 5;
            reloadSpeed = 3000;
            bulletSpeed = 10;
            damage = 20;
            range = 130;
            bulletModel = new SniperRifleBulletModel(GameData.sniperRifleBulletModel);
        }

        public override string ToString()
        {
            return "Sniper Rifle";
        }

        public override string showCurrentAmmo()
        {
            lock (GameData.weaponAccessObject)
            {
                return GameData.player.currentWeapon.ammo.ToString();
            }
        }
    }

    class Pistol : Weapon
    {
        public Pistol()
        {
            ammo = -1;
            maxAmmo = -1;
            reloadSpeed = 1000;
            bulletSpeed = 10;
            damage = 5;
            range = 15;
            bulletModel = new PistolBulletModel(GameData.pistolBulletModel);
        }

        public override string ToString()
        {
            return "Pistol";
        }

        public override string showCurrentAmmo()
        {
            lock (GameData.weaponAccessObject)
            {
                return "UNLIMITED";
            }
        }
    }
    /////////////////////////////////////////////////////////////

    //HUNTER
    class HunterZombie : Zombie
    {
        public int jumpRange;
        private Thread jumpThread;

        public HunterZombie(Pause pause) : base(pause)
        {
            health = 30;
            maxHealth = 30;
            walkSpeed = 250;
            attackDamage = 15;
            attackSpeed = 1000;
            direction = Direction.DOWN;
            jumpRange = 10;
            isPushable = true;
            jumpThread = null;
            zombieModel = new HunterZombieModel(GameData.hunterZombieModel);
            zombiePosition = findSpawnPosition();
            zombieState = new NormalState();
            GameData.addZombieAtPosition(zombiePosition, this);
            Thread zombieThread = new Thread(() => liveTemplateMethod());
            GameData.addThread(zombieThread);
            this.zombieThread = zombieThread;
            zombieThread.Start();
        }

        public override void kill()
        {
            if(jumpThread != null)
            {
                jumpThread.Interrupt();
            }
            jumpThread = null;
            zombieThread.Interrupt();
            GameData.removeThread(jumpThread);
            GameData.removeThread(zombieThread);
            GameData.zombieAtPosition.Remove(zombiePosition);
            GameData.clearPosition(zombiePosition);
        }

        public override void makeUpset()
        {
            zombieState = new AngryState();
            walkSpeed = 175;
            attackDamage = 30;
            jumpRange = 20;
            isPushable = false;
        }

        private bool isPlayerAhead()
        {
            lock (GameData.consoleAccessObject)
            {
                switch (direction)
                {
                    case Direction.UP:
                        if (GameData.player.position.x == zombiePosition.x && zombiePosition.y - GameData.player.position.y <= jumpRange)
                        {
                            if(GameData.checkForCollision(new Position(zombiePosition.x, zombiePosition.y - 1)) == Collision.NONE)
                            {
                                return true;
                            }
                        }
                        break;
                    case Direction.RIGHT:
                        if (GameData.player.position.y == zombiePosition.y && GameData.player.position.x - zombiePosition.x <= jumpRange)
                        {
                            if (GameData.checkForCollision(new Position(zombiePosition.x + 1, zombiePosition.y)) == Collision.NONE)
                            {
                                return true;
                            }
                        }
                        break;
                    case Direction.DOWN:
                        if (GameData.player.position.x == zombiePosition.x && GameData.player.position.y - zombiePosition.y <= jumpRange)
                        {
                            if (GameData.checkForCollision(new Position(zombiePosition.x, zombiePosition.y + 1)) == Collision.NONE)
                            {
                                return true;
                            }
                        }
                        break;
                    case Direction.LEFT:
                        if (GameData.player.position.y == zombiePosition.y && zombiePosition.x - GameData.player.position.x <= jumpRange)
                        {
                            if (GameData.checkForCollision(new Position(zombiePosition.x - 1, zombiePosition.y)) == Collision.NONE)
                            {
                                return true;
                            }
                        }
                        break;
                }
            }
            return false;
        }

        //+5 punktow
        protected override void addScore()
        {
            lock (GameData.playerScoreAccessObject)
            {
                GameData.player.score += 5;
            }
            GameData.playerScoreStatusEvent.Set();
        }

        //najlepsze przedmioty, podwojne opakowanie w dekorator, wysoka szansa
        protected override void dropGoodies()
        {
            int number, weaponOrPack = -1;
            lock (GameData.generateRandomNumberAccess)
            {
                weaponOrPack = GameData.generateRandomNumber(0, 100);
                number = GameData.generateRandomNumber(0, 100);
            }

            if (weaponOrPack > 50)
            {
                if (number <= 65 && number > 30)
                {
                    Drop drop = new HealthBonus(new AmmoBonus(new AmmoPack()));
                    Thread dropTrackingThread = new Thread(() => new DropTracker(pause).trackDrop(zombiePosition, drop));
                    GameData.addThread(dropTrackingThread);
                    drop.setTrackingThread(dropTrackingThread);
                    GameData.addDropAtPosition(zombiePosition, drop);
                    dropTrackingThread.Start();
                }
                else if (number <= 30)
                {
                    Drop drop = new AmmoBonus(new HealthBonus(new HealthPack()));
                    Thread dropTrackingThread = new Thread(() => new DropTracker(pause).trackDrop(zombiePosition, drop));
                    GameData.addThread(dropTrackingThread);
                    drop.setTrackingThread(dropTrackingThread);
                    GameData.addDropAtPosition(zombiePosition, drop);
                    dropTrackingThread.Start();
                }
            }
            else
            {
                if (number <= 50 && number > 25)
                {
                    Drop drop = new AK47Drop();
                    Thread dropTrackingThread = new Thread(() => new DropTracker(pause).trackDrop(zombiePosition, drop));
                    GameData.addThread(dropTrackingThread);
                    drop.setTrackingThread(dropTrackingThread);
                    GameData.addDropAtPosition(zombiePosition, drop);
                    dropTrackingThread.Start();
                }
                else if (number <= 25)
                {
                    Drop drop = new SniperRifleDrop();
                    Thread dropTrackingThread = new Thread(() => new DropTracker(pause).trackDrop(zombiePosition, drop));
                    GameData.addThread(dropTrackingThread);
                    drop.setTrackingThread(dropTrackingThread);
                    GameData.addDropAtPosition(zombiePosition, drop);
                    dropTrackingThread.Start();
                }
            }
        }

        //moze skakac do gracza
        protected override void FollowPlayer()
        {
                    if (isPlayerAround())
                    {
                        do
                        {
                            if (isPaused)
                            {
                                GameData.pauseGameEvent.Reset();
                                GameData.pauseGameEvent.WaitOne();
                            }
                            lock (GameData.consoleAccessObject)
                            {
                                if (GameData.player.health > 0)
                                {
                                    GameData.gameMechanics.hitPlayer(attackDamage);
                                    GameData.playerHealthStatusEvent.Set();
                                }
                            }
                            Thread.Sleep(attackSpeed);
                        } while (isPlayerAround());
                    }
                    else if (isPlayerAhead())
                    {
                        lock (GameData.jumpThreadAccess)
                        {
                            jumpThread = new Thread(() => new JumpTracker(pause).jump(direction, this));
                            jumpThread.Start();
                            GameData.addThread(jumpThread);
                        }
                        GameData.jumpWait.WaitOne();
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        zombieState.findNextStep(this);
                    }
        }
    }

    //TANK
    class TankZombie : Zombie
    {
        public TankZombie(Pause pause) : base(pause)
        {
            health = 100;
            maxHealth = 100;
            walkSpeed = 350;
            attackDamage = 35;
            attackSpeed = 2000;
            isPushable = false;
            direction = Direction.DOWN;
            zombieModel = new TankZombieModel(GameData.tankZombieModel);
            zombiePosition = findSpawnPosition();
            zombieState = new NormalState();
            GameData.addZombieAtPosition(zombiePosition, this);
            Thread zombieThread = new Thread(() => liveTemplateMethod());
            GameData.addThread(zombieThread);
            this.zombieThread = zombieThread;
            zombieThread.Start();
        }

        public override void kill()
        {
            zombieThread.Interrupt();
            GameData.removeThread(zombieThread);
            GameData.zombieAtPosition.Remove(zombiePosition);
            GameData.clearPosition(zombiePosition);
        }

        public override void makeUpset()
        {
            zombieState = new EnragedState();
            walkSpeed = 250;
            attackDamage = 50;
        }

        //+8 punktow
        protected override void addScore()
        {
            lock(GameData.playerScoreAccessObject)
            {
                GameData.player.score += 8;
            }
            GameData.playerScoreStatusEvent.Set();
        }

        //pewny drop paczek lub broni, pojedyncze opakowanie w bonus
        protected override void dropGoodies()
        {
            int number, weaponOrPack;
            lock (GameData.generateRandomNumberAccess)
            {
                weaponOrPack = GameData.generateRandomNumber(0, 2);
                number = GameData.generateRandomNumber(0, 2);
            }

            if (weaponOrPack == 0)
            {
                if (number == 1)
                {
                    Drop drop = new HealthBonus(new AmmoPack());
                    Thread dropTrackingThread = new Thread(() => new DropTracker(pause).trackDrop(zombiePosition, drop));
                    GameData.addThread(dropTrackingThread);
                    drop.setTrackingThread(dropTrackingThread);
                    GameData.addDropAtPosition(zombiePosition, drop);
                    dropTrackingThread.Start();
                }
                else if (number == 0)
                {
                    Drop drop = new AmmoBonus(new HealthPack());
                    Thread dropTrackingThread = new Thread(() => new DropTracker(pause).trackDrop(zombiePosition, drop));
                    GameData.addThread(dropTrackingThread);
                    drop.setTrackingThread(dropTrackingThread);
                    GameData.addDropAtPosition(zombiePosition, drop);
                    dropTrackingThread.Start();
                }
            }
            else
            {
                if (number == 1)
                {
                    Drop drop = new AK47Drop();
                    Thread dropTrackingThread = new Thread(() => new DropTracker(pause).trackDrop(zombiePosition, drop));
                    GameData.addThread(dropTrackingThread);
                    drop.setTrackingThread(dropTrackingThread);
                    GameData.addDropAtPosition(zombiePosition, drop);
                    dropTrackingThread.Start();
                }
                else if (number == 0)
                {
                    Drop drop = new SniperRifleDrop();
                    Thread dropTrackingThread = new Thread(() => new DropTracker(pause).trackDrop(zombiePosition, drop));
                    GameData.addThread(dropTrackingThread);
                    drop.setTrackingThread(dropTrackingThread);
                    GameData.addDropAtPosition(zombiePosition, drop);
                    dropTrackingThread.Start();
                }
            }
        }

        //tank moze pchnac i ogluszyc gracza
        protected override void FollowPlayer()
        {
            if (isPlayerAround())
            {
                do
                {
                    if (isPaused)
                    {
                        GameData.pauseGameEvent.Reset();
                        GameData.pauseGameEvent.WaitOne();
                    }

                    lock(GameData.consoleAccessObject)
                    {
                        if (GameData.player.health > 0)
                        {
                            GameData.gameMechanics.hitPlayer(attackDamage);
                            GameData.playerHealthStatusEvent.Set();

                            lock(GameData.playerStunnedAccess)
                            {
                                if (!GameData.player.isStunned)
                                {
                                    Thread pushAndStunThread = new Thread(() => new PushTracker(pause).pushAndStun(direction));
                                    pushAndStunThread.Start();
                                    GameData.addThread(pushAndStunThread);
                                }
                            }
                        }
                    }
                    Thread.Sleep(attackSpeed);
                } while (isPlayerAround());
            }
            else
            {
                zombieState.findNextStep(this);
            }
        }
    }

    //NORMALNY ZOMBIE
    class NormalZombie : Zombie
    {
        public NormalZombie(Pause pause) : base(pause)
        {
            health = 10;
            maxHealth = 10;
            walkSpeed = 1000;
            attackDamage = 10;
            attackSpeed = 1000;
            isPushable = true;
            direction = Direction.DOWN;
            zombieModel = new NormalZombieModel(GameData.normalZombieModel);
            zombiePosition = findSpawnPosition();
            zombieState = new NormalState();
            GameData.addZombieAtPosition(zombiePosition, this);
            Thread zombieThread = new Thread(() => liveTemplateMethod());
            GameData.addThread(zombieThread);
            this.zombieThread = zombieThread;
            zombieThread.Start();
        }

        public override void kill()
        {
            zombieThread.Interrupt();
            GameData.removeThread(zombieThread);
            GameData.zombieAtPosition.Remove(zombiePosition);
            GameData.clearPosition(zombiePosition);
        }

        public override void makeUpset()
        {
            zombieState = new AngryState();
            walkSpeed = 500;
            attackDamage = 20;
            isPushable = false;
        }

        //+1 punkt
        protected override void addScore()
        {
            lock(GameData.playerScoreAccessObject)
            {
                GameData.player.score += 1;
            }
            GameData.playerScoreStatusEvent.Set();
        }

        //najslabsze przedmioty, brak opakowania w dekorator, slaba szansa
        protected override void dropGoodies()
        {
            int number, weaponOrPack = -1;
            lock (GameData.generateRandomNumberAccess)
            {
                weaponOrPack = GameData.generateRandomNumber(0, 100);
                number = GameData.generateRandomNumber(0, 100);
            }

            if (weaponOrPack > 20)
            {
                if (number <= 35 && number > 10)
                {
                    Drop drop = new AmmoPack();
                    Thread dropTrackingThread = new Thread(() => new DropTracker(pause).trackDrop(zombiePosition, drop));
                    GameData.addThread(dropTrackingThread);
                    drop.setTrackingThread(dropTrackingThread);
                    GameData.addDropAtPosition(zombiePosition, drop);
                    dropTrackingThread.Start();
                }
                else if (number <= 10)
                {
                    Drop drop = new HealthPack();
                    Thread dropTrackingThread = new Thread(() => new DropTracker(pause).trackDrop(zombiePosition, drop));
                    GameData.addThread(dropTrackingThread);
                    drop.setTrackingThread(dropTrackingThread);
                    GameData.addDropAtPosition(zombiePosition, drop);
                    dropTrackingThread.Start();
                }
            }
            else
            {
                if (number <= 35 && number > 10)
                {
                    Drop drop = new AK47Drop();
                    Thread dropTrackingThread = new Thread(() => new DropTracker(pause).trackDrop(zombiePosition, drop));
                    GameData.addThread(dropTrackingThread);
                    drop.setTrackingThread(dropTrackingThread);
                    GameData.addDropAtPosition(zombiePosition, drop);
                    dropTrackingThread.Start();
                }
                else if (number <= 10)
                {
                    Drop drop = new SniperRifleDrop();
                    Thread dropTrackingThread = new Thread(() => new DropTracker(pause).trackDrop(zombiePosition, drop));
                    GameData.addThread(dropTrackingThread);
                    drop.setTrackingThread(dropTrackingThread);
                    GameData.addDropAtPosition(zombiePosition, drop);
                    dropTrackingThread.Start();
                }
            }
        }

        //nie ma specjalnych umiejetnosci jak tank lub hunter
        protected override void FollowPlayer()
        {
                    if (isPlayerAround())
                    {
                        do
                        {
                            if (isPaused)
                            {
                                GameData.pauseGameEvent.Reset();
                                GameData.pauseGameEvent.WaitOne();
                            }
                            lock (GameData.consoleAccessObject)
                            {
                                if (GameData.player.health > 0)
                                {
                                    GameData.gameMechanics.hitPlayer(attackDamage);
                                    GameData.playerHealthStatusEvent.Set();
                                }
                            }
                            Thread.Sleep(attackSpeed);
                        } while (isPlayerAround());
                    }
                    else
                    {
                        zombieState.findNextStep(this);
                    }
        }
    }
    /////////////////////////////////////////////////////////////

    //WZORZEC ITERATOR - LISTA WYNIKOW//
    interface Iterator
    {
        bool hasNextPage();
        bool hasPreviousPage();
        void nextPage();
        void previousPage();
        object getPage();
    }

    //IMPLEMENTUJE PORUSZANIE SIE PO STRONACH SCOREBOARD'U
    class ScoreIterator : Iterator
    {
        private ScoreAggregate aggregate;
        private int current;

        public ScoreIterator(ScoreAggregate aggregate)
        {
            this.aggregate = aggregate;
            current = 0;
        }

        public object getPage()
        {
            return aggregate[current];
        }

        public bool hasNextPage()
        {
            if(((ICollection<Score>)aggregate[current+1]).Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool hasPreviousPage()
        {
            if(current == 0)
            {
                return false;
            }

            if (((ICollection<Score>)aggregate[current - 1]).Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void nextPage()
        {
            current += 1;
        }

        public void previousPage()
        {
            current -= 1;
        }
    }

    //OBIEKT WYNIKU, PRZECHOWUJE WYNIK GRACZA I POZIOM TRUDNOSCI
    class Score
    {
        public Score(string playerName, string playerScore, string gameDifficulty)
        {
            this.playerName = playerName;
            this.playerScore = playerScore;
            this.gameDifficulty = gameDifficulty;
        }

        public string playerName;
        public string playerScore;
        public string gameDifficulty;

        public override string ToString()
        {
            return playerName + " " + playerScore + " " + gameDifficulty;
        }
    }

    interface Aggregate
    {
        Iterator createIterator();
    }

    //KOLEKCJA WYNIKOW
    class ScoreAggregate : Aggregate
    {
        private List<Score> scoreList;

        public ScoreAggregate()
        {
            scoreList = new List<Score>();
        }

        public Iterator createIterator()
        {
            return new ScoreIterator(this);
        }

        public void addScore(Score score)
        {
            scoreList.Add(score);
        }

        //zwraca strone o podanym numerze
        public object this[int index]
        {
            get
            {
                try
                {
                    return scoreList.GetRange(index * 10, 10);
                }
                catch (ArgumentException e)
                {
                    if(scoreList.Count - index * 10 < 0)
                    {
                        return new List<Score>();
                    }

                    return scoreList.GetRange(index * 10, scoreList.Count - index * 10);
                }
            }
        }
    }

    //ROZNE MENU WYSTEPUJACE W GRZE
    static class Menus
    {
        //interfejs menu glownego
        public static void mainMenu()
        {
            ConsoleKeyInfo keyInfo;
            string[] options = new string[] { "START", "SCOREBOARD", "HOW TO PLAY", "OPTIONS", "CREDITS", "QUIT" };
            int ptr = 0;
            GameData.option = options[ptr];
            int y;
            Console.ResetColor();
            Console.Clear();
            GameData.drawText();
            do
            {
                y = 15;

                Console.SetCursorPosition(60, y);
                foreach (var option in options)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    if (options[ptr] == option)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;


                    }
                    Console.SetCursorPosition(60, y++);
                    Console.Write(option);
                }

                keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (ptr - 1 == -1)
                        {
                            ptr = 5;
                            GameData.option = options[ptr];
                            Console.ResetColor();
                        }
                        else
                        {
                            GameData.option = options[ptr--];
                        }
                        Console.ResetColor();
                        break;
                    case ConsoleKey.DownArrow:
                        if (ptr + 1 == 6)
                        {
                            ptr = 0;
                            GameData.option = options[ptr];
                        }
                        else
                        {
                            GameData.option = options[ptr++];
                        }
                        Console.ResetColor();
                        break;
                }
            } while (keyInfo.Key != ConsoleKey.Enter);
            GameData.option = options[ptr];
            Console.Clear();


            if (GameData.option == "SCOREBOARD")
            {
                Menus.scoreboardMenu();
            }
            if (GameData.option == "QUIT")
            {
                Environment.Exit(0);
            }
            if (GameData.option == "CREDITS")
            {
                Menus.creditsMenu();
            }
            if (GameData.option == "OPTIONS")
            {
                Menus.optionsMenu();
            }
            if (GameData.option == "HOW TO PLAY")
            {
                Menus.howToPlayMenu();
            }
            if (GameData.option == "START")
            {
                Menus.nameInputMenu();
                GameData.initializeGame();
                GameData.exitGame.Reset();
                GameData.exitGame.WaitOne();
            }
        }

        //informacje odnosnie gry, jej tworcow itd...
        public static void creditsMenu()
        {

            Console.Clear();
            GameData.drawText();
            ConsoleKeyInfo keyInfo;
            Console.SetCursorPosition(35, 15);
            Console.WriteLine("GRA ZOSTALA STWORZONA PRZEZ :");
            Console.SetCursorPosition(35, 16);
            Console.WriteLine();
            Console.SetCursorPosition(35, 17);
            Console.WriteLine("KAMIL KAPLINSKI ORAZ LUKASZ HRYNIEWICKI");
            Console.SetCursorPosition(35, 18);
            Console.WriteLine();
            Console.SetCursorPosition(35, 19);
            Console.WriteLine("SPECJALNE PODZIEKOWANIA DLA ZSAKUL REMODAG ZA OKAZJE DO STWORZENIA TEJ PIEKNEJ GRY");
            Console.SetCursorPosition(35, 20);
            Console.WriteLine();
            Console.SetCursorPosition(53, 35);
            Console.WriteLine("ESC - BACK TO MAIN MENU");
            do
            {
                keyInfo = Console.ReadKey(true);

            } while (keyInfo.Key != ConsoleKey.Escape);
        }

        //wybor poziomu trudnosci gry
        public static void difficultyMenu()
        {
            ConsoleKeyInfo keyInfo;
            string[] options = new string[] { "EASY", "MEDIUM", "HARD"};
            int ptr = 0;
            GameData.option = options[ptr];
            int y;
            bool done = false;
            Console.ResetColor();
            Console.Clear();
            Console.SetCursorPosition(55, 35);
            Console.WriteLine("ESC - BACK TO OPTIONS");
            GameData.drawText();
            do
            {
                do
                {
                    y = 15;
                    Console.SetCursorPosition(60, y);
                    foreach (var option in options)
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        if (options[ptr] == option)
                        {
                            Console.BackgroundColor = ConsoleColor.White;
                            Console.ForegroundColor = ConsoleColor.Black;


                        }
                        Console.SetCursorPosition(60, y++);
                        Console.Write(option);
                    }

                    keyInfo = Console.ReadKey(true);
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.UpArrow:
                            if (ptr - 1 == -1)
                            {
                                ptr = 2;
                                GameData.option = options[ptr];
                                Console.ResetColor();
                            }
                            else
                            {
                                GameData.option = options[ptr--];
                            }
                            Console.ResetColor();
                            break;
                        case ConsoleKey.DownArrow:
                            if (ptr + 1 == 3)
                            {
                                ptr = 0;
                                GameData.option = options[ptr];
                            }
                            else
                            {
                                GameData.option = options[ptr++];
                            }
                            Console.ResetColor();
                            break;
                    }
                } while (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Escape);

                if(keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.ResetColor();
                    break;
                }

                GameData.option = options[ptr];


                switch (GameData.option)
                {
                    case "EASY":
                        GameData.difficulty = "Easy";
                        Console.SetCursorPosition(60, 25);
                        Console.ResetColor();
                        Console.Write("                               ");
                        Console.SetCursorPosition(60, 25);
                        Console.Write("Difficulty set to easy");
                        break;
                    case "MEDIUM":
                        GameData.difficulty = "Medium";
                        Console.SetCursorPosition(60, 25);
                        Console.ResetColor();
                        Console.Write("                               ");
                        Console.SetCursorPosition(60, 25);
                        Console.Write("Difficulty set to medium");
                        break;
                    case "HARD":
                        GameData.difficulty = "Hard";
                        Console.SetCursorPosition(60, 25);
                        Console.ResetColor();
                        Console.Write("                               ");
                        Console.SetCursorPosition(60, 25);
                        Console.Write("Difficulty set to hard");
                        break;
                }

            } while (!done);
        }

        //opcje graficzne gry(ramka oraz background)
        public static void graphicsMenu()
        {
            ConsoleKeyInfo keyInfo;
            string[] options = new string[] { "DEFAULT", "MATHEMATHICS", "RAINBOW"};
            int ptr = 0;
            GameData.option = options[ptr];
            int y;
            bool done = false;
            Console.ResetColor();
            Console.Clear();
            Console.SetCursorPosition(55, 35);
            Console.WriteLine("ESC - BACK TO OPTIONS");
            GameData.drawText();
            do
            {
                do
                {
                    y = 15;

                    Console.SetCursorPosition(60, y);
                    foreach (var option in options)
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        if (options[ptr] == option)
                        {
                            Console.BackgroundColor = ConsoleColor.White;
                            Console.ForegroundColor = ConsoleColor.Black;


                        }
                        Console.SetCursorPosition(60, y++);
                        Console.Write(option);
                    }

                    keyInfo = Console.ReadKey(true);
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.UpArrow:
                            if (ptr - 1 == -1)
                            {
                                ptr = 2;
                                GameData.option = options[ptr];
                                Console.ResetColor();
                            }
                            else
                            {
                                GameData.option = options[ptr--];
                            }
                            Console.ResetColor();
                            break;
                        case ConsoleKey.DownArrow:
                            if (ptr + 1 == 3)
                            {
                                ptr = 0;
                                GameData.option = options[ptr];
                            }
                            else
                            {
                                GameData.option = options[ptr++];
                            }
                            Console.ResetColor();
                            break;
                    }
                } while (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Escape);

                if(keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.ResetColor();
                    break;
                }

                GameData.option = options[ptr];

                switch (GameData.option)
                {
                    case "DEFAULT":
                        GameData.interfaceBuilder = "Default";
                        Console.SetCursorPosition(60, 25);
                        Console.ResetColor();
                        Console.Write("                               ");
                        Console.SetCursorPosition(60, 25);
                        Console.Write("Frame style set to default");
                        break;
                    case "MATHEMATHICS":
                        GameData.interfaceBuilder = "Mathemathics";
                        Console.SetCursorPosition(60, 25);
                        Console.ResetColor();
                        Console.Write("                               ");
                        Console.SetCursorPosition(60, 25);
                        Console.Write("Frame style set to mathemathics");
                        break;
                    case "RAINBOW":
                        GameData.interfaceBuilder = "Rainbow";
                        Console.ResetColor();
                        Console.SetCursorPosition(60, 25);
                        Console.Write("                               ");
                        Console.SetCursorPosition(60, 25);
                        Console.Write("Frame style set to rainbow");
                        break;
                }

            } while (!done);
        }

        //opcje gry
        public static void optionsMenu()
        {
            ConsoleKeyInfo keyInfo;
            string[] options = new string[] { "DIFFICULTY", "GRAPHICS"};
            int ptr = 0;
            GameData.option = options[ptr];
            int y;
            bool done = false;
            do
            {
                Console.Clear();
                GameData.drawText();
                Console.ResetColor();
                Console.SetCursorPosition(53, 35);
                Console.WriteLine("ESC - BACK TO MAIN MENU");
                do
                {
                    y = 15;
                    Console.SetCursorPosition(60, y);
                    foreach (var option in options)
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        if (options[ptr] == option)
                        {
                            Console.BackgroundColor = ConsoleColor.White;
                            Console.ForegroundColor = ConsoleColor.Black;


                        }
                        Console.SetCursorPosition(60, y++);
                        Console.Write(option);
                    }

                    keyInfo = Console.ReadKey(true);
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.UpArrow:
                            if (ptr - 1 == -1)
                            {
                                ptr = 1;
                                GameData.option = options[ptr];
                                Console.ResetColor();
                            }
                            else
                            {
                                GameData.option = options[ptr--];
                            }
                            Console.ResetColor();
                            break;
                        case ConsoleKey.DownArrow:
                            if (ptr + 1 == 2)
                            {
                                ptr = 0;
                                GameData.option = options[ptr];
                            }
                            else
                            {
                                GameData.option = options[ptr++];
                            }
                            Console.ResetColor();
                            break;
                    }
                } while (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Escape);

                if(keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.ResetColor();
                    break;
                }

                GameData.option = options[ptr];
                Console.Clear();

                switch (GameData.option)
                {
                    case "DIFFICULTY":
                        Menus.difficultyMenu();
                        break;
                    case "GRAPHICS":
                        Menus.graphicsMenu();
                        break;
                }

            } while (!done);
        }

        //manual odnosnie sterowania itp...
        public static void howToPlayMenu()
        {
            Console.Clear();
            GameData.drawText();
            ConsoleKeyInfo keyInfo;
            Console.SetCursorPosition(60, 15);
            Console.WriteLine("HOW TO PLAY");
            Console.SetCursorPosition(15, 17);
            Console.WriteLine("MOVEMENT:");
            Console.SetCursorPosition(15, 19);
            Console.WriteLine("LEFT - ←");
            Console.SetCursorPosition(15, 20);
            Console.WriteLine("RIGHT - →");
            Console.SetCursorPosition(15, 21);
            Console.WriteLine("UP - ↑");
            Console.SetCursorPosition(15, 22);
            Console.WriteLine("DOWN - ↓");
            Console.SetCursorPosition(15, 23);
            Console.WriteLine("SHOOT - SPACEBAR");
            Console.SetCursorPosition(15, 24);
            Console.WriteLine("PISTOL - 1");
            Console.SetCursorPosition(15, 25);
            Console.WriteLine("AK47 - 2");
            Console.SetCursorPosition(15, 26);
            Console.WriteLine("SNIPER RIFLE - 3");
            Console.SetCursorPosition(53, 35);
            Console.WriteLine("ESC - BACK TO MAIN MENU");

            Console.SetCursorPosition(35, 17);
            Console.WriteLine("ZOMBIES:");
            Console.SetCursorPosition(35, 19);
            Console.WriteLine("@ - Normal zombie, slow movement, little damage,");
            Console.SetCursorPosition(40, 20);
            Console.WriteLine("litle health, low drop rate");
            Console.SetCursorPosition(35, 22);
            Console.WriteLine("Y - Tank, fast movement, huge damage, huge health,");
            Console.SetCursorPosition(40, 23);
            Console.WriteLine("can stun and push you away, high drop rate");
            Console.SetCursorPosition(35, 25);
            Console.WriteLine("x - Hunter, very fast movement, medium damage,");
            Console.SetCursorPosition(40, 26);
            Console.WriteLine("medium health, can jump to you, medium drop rate");


            Console.SetCursorPosition(90, 17);
            Console.Write("ITEMS:");
            Console.SetCursorPosition(90, 19);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("#");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" - Ammo pack");
            Console.SetCursorPosition(90, 20);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("+");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" - Health pack");
            Console.SetCursorPosition(90, 21);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("A");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" - AK47");
            Console.SetCursorPosition(90, 22);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("S");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" - Sniper Rifle");
            Console.SetCursorPosition(90, 23);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("#");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" - Yellow foreground on drop");
            Console.SetCursorPosition(94, 24);
            Console.Write("stands for bonus ammo");
            Console.SetCursorPosition(90, 25);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Green;
            Console.Write("+");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" - Green background on drop");
            Console.SetCursorPosition(94, 26);
            Console.Write("stands for bonus health");

            do
            {
                keyInfo = Console.ReadKey(true);

            } while (keyInfo.Key != ConsoleKey.Escape);
        }

        //interfejs wpisywania imienia gracza przy starcie rozgrywki
        public static void nameInputMenu()
        {
            string name = " ";
            Console.Clear();
            Console.SetCursorPosition(60, 20);
            Console.Write("Please enter Your name");
            Console.SetCursorPosition(60, 24);
            Console.Write("*Your name cannot contain white spaces");
            Console.SetCursorPosition(60, 22);
            Console.Write("Name: ");
            do
            {
                Console.SetCursorPosition(66, 22);
                foreach (char c in name)
                {
                    Console.Write(" ");
                }
                Console.SetCursorPosition(66, 22);
                name = Console.ReadLine();
                GameData.playerName = name;
            } while (String.IsNullOrWhiteSpace(name) || name.Contains(" "));
        }

        //tabela wynikow
        public static void scoreboardMenu()
        {
            Console.Clear();
            ConsoleKeyInfo keyInfo;
            GameData.drawText();
            string line;
            string[] data = new string[3];
            int pageNumber = 1;
            Score score;

            Console.SetCursorPosition(55, 15);
            Console.Write("Name: ");
            Console.Write("Score: ");
            Console.Write("Difficulty: ");

            Console.SetCursorPosition(47, 40);
            Console.Write("Previous page: <-      ");
            Console.Write("Next page: ->");
            Console.SetCursorPosition(62, 42);
            Console.Write("Page " + pageNumber);
            Console.SetCursorPosition(53, 45);
            Console.Write("ESC - BACK TO MAIN MENU");

            int x = 55, y = 17;

            ScoreAggregate scoreAggregate = new ScoreAggregate();

            try
            {
                StreamReader file = new StreamReader(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\ranking.txt");

                while ((line = file.ReadLine()) != null)
                {
                    data = line.Split(' ');
                    score = new Score(data[0], data[1], data[2]);
                    scoreAggregate.addScore(score);
                }
                file.Close();

                //stworzenie iteratora i wypisanie pojedynczej, pierwszej strony
                Iterator scoreIterator = scoreAggregate.createIterator();

                Console.SetCursorPosition(x, y);
                foreach(Score s in ((IEnumerable<Score>)scoreIterator.getPage()))
                {
                    Console.Write(s);
                    y += 2;
                    Console.SetCursorPosition(x, y);
                }

                //poruszanie sie po liscie wynikow
                do
                {
                    keyInfo = Console.ReadKey(true);
                    y = 17;
                    switch(keyInfo.Key)
                    {
                        case ConsoleKey.RightArrow:
                            if(scoreIterator.hasNextPage())
                            {
                                pageNumber += 1;
                                GameData.clearText(new Position(0, 17), new Position(130, 39));
                                scoreIterator.nextPage();
                                Console.SetCursorPosition(x, y);
                                foreach (Score s in ((IEnumerable<Score>)scoreIterator.getPage()))
                                {
                                    Console.Write(s);
                                    y += 2;
                                    Console.SetCursorPosition(x, y);
                                }
                                Console.SetCursorPosition(67, 42);
                                Console.Write(pageNumber);
                            }
                            break;
                        case ConsoleKey.LeftArrow:
                            if (scoreIterator.hasPreviousPage())
                            {
                                pageNumber -= 1;
                                GameData.clearText(new Position(0, 17), new Position(130, 39));
                                scoreIterator.previousPage();
                                Console.SetCursorPosition(x, y);
                                foreach (Score s in ((IEnumerable<Score>)scoreIterator.getPage()))
                                {
                                    Console.Write(s);
                                    y += 2;
                                    Console.SetCursorPosition(x, y);
                                }
                                Console.SetCursorPosition(67, 42);
                                Console.Write("         ");
                                Console.SetCursorPosition(67, 42);
                                Console.Write(pageNumber);
                            }
                            break;
                    }
                } while (keyInfo.Key != ConsoleKey.Escape);

            }
            catch (FileNotFoundException e) { }
        }

        //interfejs menu po smierci postaci 
        public static void gameOverMenu()
        {
            Console.Clear();
            string[] options = new string[] { "TRY AGAIN", "EXIT TO MAIN MENU" };
            int ptr = 0;
            GameData.option = options[ptr];
            int x;
            string chain = GameData.player.Name + ", Your score is: " + GameData.player.score;

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(60, 18);
            Console.Write("GAME OVER");

            Console.SetCursorPosition(64 - chain.Length / 2, 20);
            Console.Write(chain);

            do
            {
                x = 51;
                Console.SetCursorPosition(x, 22);
                foreach (var option in options)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    if (options[ptr] == option)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.SetCursorPosition(x, 24);
                    Console.Write(option);
                    x += options[0].Length + 1;
                }

                GameData.keyInfoWaitEvent.Reset();
                GameData.keyInfoWaitEvent.WaitOne();

                switch (GameData.keyInfo.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (ptr - 1 == -1)
                        {
                            ptr = 1;
                            GameData.option = options[ptr];
                            Console.ResetColor();
                        }
                        else
                        {
                            GameData.option = options[ptr--];
                        }
                        Console.ResetColor();
                        break;
                    case ConsoleKey.RightArrow:
                        if (ptr + 1 == 2)
                        {
                            ptr = 0;
                            GameData.option = options[ptr];
                        }
                        else
                        {
                            GameData.option = options[ptr++];
                        }
                        Console.ResetColor();
                        break;
                }
            } while (GameData.keyInfo.Key != ConsoleKey.Enter);
            GameData.option = options[ptr];
        }

        //interfejs menu pauzy (podczas rozgrywki)
        public static void pauseMenu()
        {
            ConsoleKeyInfo keyInfo;
            string[] options = new string[] { "RESUME", "EXIT TO MAIN MENU" };
            int ptr = 0;
            GameData.option = options[ptr];
            int x;

            Console.BackgroundColor = GameData.backgroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(110, 26);
            Console.Write("GAME IS PAUSED");

            do
            {
                x = 105;
                Console.SetCursorPosition(x, 28);
                foreach (var option in options)
                {
                    Console.BackgroundColor = GameData.backgroundColor;
                    Console.ForegroundColor = ConsoleColor.White;
                    if (options[ptr] == option)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.SetCursorPosition(x, 28);
                    Console.Write(option);
                    x += options[0].Length + 1;
                }

                keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (ptr - 1 == -1)
                        {
                            ptr = 1;
                            GameData.option = options[ptr];
                            Console.ResetColor();
                        }
                        else
                        {
                            GameData.option = options[ptr--];
                        }
                        Console.ResetColor();
                        break;
                    case ConsoleKey.RightArrow:
                        if (ptr + 1 == 2)
                        {
                            ptr = 0;
                            GameData.option = options[ptr];
                        }
                        else
                        {
                            GameData.option = options[ptr++];
                        }
                        Console.ResetColor();
                        break;
                }
            } while (keyInfo.Key != ConsoleKey.Enter);
            GameData.option = options[ptr];

            Console.BackgroundColor = GameData.backgroundColor;
        }
    }
    /////////////////////////////////////////////////////////////

    class Program
    {
        static void Main(string[] args)
        {
            GameData.exitGame = new ManualResetEvent(false);
            Console.SetWindowSize(130, 50);
            Console.CursorVisible = false;

            while (true)
            {
                Menus.mainMenu();
            }
        }
    }
}