# Left4Dead
A computer game created for the purpose of passing the Advanced Programming Techniques course. The purpose of the project was to implement several programming patterns. As a player you have to survive waves of zombies that want to kill you.

Main menu

![image](https://user-images.githubusercontent.com/47573956/164887607-0e30ad88-b41a-453a-8fcf-2014613d7991.png)

How to play:

![image](https://user-images.githubusercontent.com/47573956/164889240-fe79f9ea-c6ec-4424-8c25-a6c62484b4b4.png)

Game interface

![image](https://user-images.githubusercontent.com/47573956/164887631-b149d8d3-3de7-4e7c-85ef-a42209c78996.png)

### Used programing patterns
The project includes the following patterns:
#### Singleton
- Use case: There is one only one player in game. Player object should be created only at the beginning of the game
- `Player` class is singleton
- Lazy initialization: create only when game starts
- When game ends, player object is disposed
- UML diagram:

  ![image](https://user-images.githubusercontent.com/47573956/164887732-0ab4d431-a1b4-4ef7-acbf-ce45dba9b3e7.png)

#### Builder
- Use case: user should be able to change interface look & feel
- `InterfaceDirector` directs the building process using `IInterfaceBuilder`
- There are three builder implementations:
  - `DefaultInterfaceBuilder`
  - `MathematicsInterfaceBuilder`
  
    ![image](https://user-images.githubusercontent.com/47573956/164888243-b26f9172-9ef9-4769-a6f4-7bc3796c9803.png)
    
  - `RainbowInterfaceBuilder`

    ![image](https://user-images.githubusercontent.com/47573956/164888190-862f63a6-f1b2-4b39-85bb-c146047fb748.png)
- Builder creates `Product` class that contains interface look & feel properties
- UML diagram:

  ![image](https://user-images.githubusercontent.com/47573956/164888685-895cb13f-f9e5-4911-bcdb-370e2957fdc5.png)

#### Decorator
- Use case: zombies should drop loot. Dropped objects should vary in attributes. Harder to kill zombies should drop more appealing loot. What matters is flexibility and complete freedom in loot design.
- There are different objects that zombies drop: Sniper rifles (`SniperRifleDrop`), AK47's (`AK47Drop`), Health packs (`HealthPack`), Ammo packs (`AmmoPack`)
- `Bonus` class is a decorator that adds additional value to dropped items
- UML diagram:

  ![image](https://user-images.githubusercontent.com/47573956/164888725-69187e19-c355-4281-92f4-b16e89d3d2b5.png)

#### Strategy
- Use case: player can change game difficulty. Different difficulties should affect gameplay. Right now it affects zombie spawn rate and zombie kind
- There are three difficulties:
    - Easy: only normal zombies are being spawned. Spawn interval is constant - it doesn't change throughout the game
    - Medium: normal and tank zombies are spawned. Zombie kind is chosen based upon player score. The better player is, more tanks will be spawned. Spawn interval is random
    - Hard: normal, tank and hunter zombies are spawned. Spawn interval and zombie kind depend on player score and his health. If player is at high health, more zombies are spawned
- `ZombieMaker` is a context class that is responsible for creating zombies
- Spawn interval and zombie kind is chosen in difficulty classes: `EasyDifficulty`, `MediumDifficulty` and `HardDifficulty` classes
- The context class is passed to difficulty implementations that modify its properties
- UML diagram:
  
  ![image](https://user-images.githubusercontent.com/47573956/164889201-f71f44f4-a5af-45dc-b9ca-b0649027efe9.png)

#### Template method
- Use case: Although there are different kind of zombies, each with unique abilities, their life-cycle is the same. Their goal is to follow player and hurt him
- `Zombie` is an abstract class containing `LiveTemplateMethod` method
- There are three kind of zombies:
  - Normal
  - Hunter: can jump from a long distance to player location
  - Tank: can throw player on a long distance. A player is stunned for a short time upon stopping
- Concrete zombies (Tank, Hunter, Normal) override life-cycle abilities to drop loot (`DropGoodies`), add player score upon death (`AddScore`) and follow player (`FollowPlayer`)
- UML diagram:
  
  ![image](https://user-images.githubusercontent.com/47573956/164889490-9650687c-2934-4168-97fd-94ee648e0ea8.png)

#### State
  - Use case: zombies can change their behavior based on their health points
  - There are three states that zombie can be in:
    - Normal: the usual zombie behavior
    - Angry: only normal and hunter can enter this state, a zombie can also move diagonal now, can push other zombies that are not angry
    - Enraged: only Tank can enter this state, can also move diagonal now, instead of pushing zombies away, tank kills other zombies that are on his way to player
  - A zombie can only transition from normal state to either angry or enraged
  - Each state implementation can trigger a transition to another state based upon zombie health using `MakeUpset` zombie method
  - UML diagram:

    ![image](https://user-images.githubusercontent.com/47573956/164890649-b7b64d50-e142-464c-af39-5b4d14594cb5.png)
  
#### Observer
  - Use case: The game architecture is based on threads. Every zombie in game is a different thread. Also push tracker that tracks player stun time, jump tracker that tracks hunter flight, or zombie maker, these objects are also threads. There is a need for notify mechanism which tells threads when game pauses so that they will sleep and wait until game is resumed
  - `Observer` represents an object that can be pasued, `Subject` allows for observer subscription
  - UML diagram:
 
    ![image](https://user-images.githubusercontent.com/47573956/164891170-b63d66a9-1a50-484d-8356-4696ce746864.png)

#### Iterator
  - Use case: Player scoreboard should be pageable
    ![image](https://user-images.githubusercontent.com/47573956/164891277-11b9074f-5bcb-4181-8ea0-b6fcf45baf88.png)
  - There are 10 scores on a single page. `GetPage` returns at most 10 scores
  - `ScoreAggregate` contains list of all scores
  - UML diagram:
 
    ![image](https://user-images.githubusercontent.com/47573956/164891356-514e217c-ac89-42ce-95a8-0ca3ad514982.png)


