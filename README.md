Prototipo extendiendo el Unity FPS Microgame, para el Guerrilla Game Dev Contest.

Modificaciones implementadas:

Mecánicas:

- Propulsión: hecha a partir de la mecánica de Quake, hace que cuando el jugador dispara a una supercifie estando cerca, recibe un impulso en la dirección contraria, lo que sirve, por ejemplo, para impulsarse hacia arriba y llegar más alto que con un salto normal.

- Gancho: se activa apuntando a un muro o a un enemigo y pulsando la tecla R. Se lanza un gancho con una cuerda al objetivo, y el jugador sale disparado hacia esa dirección. se puede desenganchar el objetivo en cualquier momento pulsando la tecla T. Mientras el jugador se acerca al enemigo enganchado, es invulnerable, pero él puede dispararle al enemigo, en movimiento.

- Patada con Gancho: cuando el jugador engancha a un enemigo y se esté acercando hacia él, la cuerda se pone roja por un momento, antes de alcanzar al enemigo. Si pulsa el botón de saltar (tecla Espacio), le pegará una patada al enemigo causándole mucho más daño que con un disparo, y el jugador recibirá un impulso hacia arriba.

Estas mecánicas hacen el juego mucho más divertido de lo que era en la versión inicial, poniendo el foco del gameplay en la movilidad constante.


Punto de entrada al juego en el proyecto de Unity: Assets\FPS\Scenes\IntroMenu


Niveles:

- Nivel 1: Se explica la propulsión, es muy corto, a modo de tutorial.
- Nivel 2: Se explica el gancho y la patada, y luego continua con varias secciones con enemigos.


Recursos externos usados:

Efectos de sonido del gancho: 
- https://freesound.org/people/PNMCarrieRailfan/sounds/682371/
- https://freesound.org/people/16bitstudios/sounds/541975/

Modelo 3D de la pierna:
- https://sketchfab.com/3d-models/sci-fi-female-soldier-lowpoly-6d23775b6f1447a39b70deeb29d9b959

Modelo 3D del gancho:
- https://sketchfab.com/3d-models/grappling-hook-86e25eac0bce48f8a03bacf48c4ec468

Scripts modificados:
- PlayerCharacterController (implementadas las mecánicas descritas anteriormente)
- PlayerInputHandler (para registrar el input de las teclas asociadas a las nuevas mecánicas)
- PlayerWeaponsManager (para añadir un evento al disparar, que se usa para añadir el impulso en PlayerCharacterController)

Scripts creados:
- CameraShake: (modificado a partir de https://gist.github.com/ftvs/5822103)
- HookHead: para mostrar visualmente el gancho
- HookPickUp: para hacer que la mecanica del gancho y la patada se desbloqueen con un objeto recogible.
- Level1Tutorial / Level2Tutorial: para mostrar en el HUD un tutorial básico de cada mecánica.





