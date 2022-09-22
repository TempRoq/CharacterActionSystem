# CharacterActionSystem
An easy to use move creator intended for action oriented genres, such as fighting or beat 'em up games, made for Unity. 


+ Hitboxes
    - Handle damage, knockback, hitstun, hitstop, lag, attack linking, etc. innately
    - Can be modified to send whatever values in an attack the user deems necessary.

+ Hitbox Clusters
    - Handle hitbox priority, sound effects on miss, and type references to be transmitted by different types of hitbox clusters. User can add this to by appending to the hitbox cluster class.

+ Actions
    - Handles endlag and landing lag, what the move links into and with under what circumstances, and major state modifications, such as negating gravity on use.

+ Action Handler
    - Parses and executes actions that cast to designated collision layers.

+ Hitbox Receiver
    - Receives information from hitboxes and sends them to necessary scripts.

