# OpenMod-Plugins
A collection of my plugins for [OpenMod](https://github.com/openmod/openmod)/Unturned.
## [Bl0721e.Behicle](Bl0721e.Behicle)
Command to install:
```
openmod install Bl0721e.Behicle
```
- Protects locked vehicles from other players.
- Allow zombies and the owner to damage the vehicle.
- Restricts item usage on other players' vehicles.
- Limits how many vehicles a player can lock.
- Tags locked vehicle 'not naturally spawned' so the game ignores them when checking vehicle amount and spawns more vehicles.
### TODO:
- automatically unlock locked vehicles left in towns
## [Bl0721e.NoRaid](Bl0721e.NoRaid)
Command to install:
```
openmod install Bl0721e.NoRaid
```
- Protects buildables from other players.
- Allow zombies and owner to damage the buildables.
- Allow players damage all buildables placed in zombie activity areas.
- Allow players damage all buildables placed on vehicles that are owned by the player or unlocked.
- Configurable timeout. Players can destruct structures that have timed out.
## [Bl0721e.Common](Bl0721e.Common)
This is automatically installed as a dependency of other plugins.  
Library for other plugins.  
Provides locale supports.  
## [Deprecated][RejectConfOverride](Blockie.RejectConfOverride)
Command to install:
```
openmod install Blockie.RejectConfOverride
```
This one is deprecated.   
Disables per map config overriding.  
Could be useful if you want to enable on vehicle building on some maps that doesnt allow you to like Arid or Buak.  
We can now achieve same effect with the new txt config file.  
