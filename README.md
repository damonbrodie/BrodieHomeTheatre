# Brodie Home Theatre

This project was developed to meet the needs for my [home theatre](http://www.avsforum.com/forum/19-dedicated-theater-design-construction/1033681-brodie-home-theatre-build-thread-2.html#post46048545) where I wanted very selective home automation to make the experience as seemless as possible.

The project incorporates the following technologies: 
 - Insteon light dimmers that provide a programmable lighting interface alongside the standard light dimmer
 - Harmony Remote controls to allow the use to control media playback, but also allow automation certain functions
 - Microsoft Kinect that provides a beamforming array microphone that allows room scale "distant" voice recognition as well as a camera that can be used to determine when humans are present in the room.  This is used as a room occupancy sensor
 - The brain of my theatre is an HTPC running Kodi where I do all of my media playback.  Kodi has a plugin framework and I use that to understand the state of the media player to the automation framework (this project).

This project forms an automation application that runs on the HTPC in the theatre that listens to the events and uses them as decision points to signal events.

 ## Use Cases ##
This project aims to provide very specific automation to both enhance the *cool* factor of the room as well as make the experience of use the theatre.  The following high level use cases are supported by this application:
- Upon entering the room, bring up the room lights
- Use speech recognition to listen for voice requests to turn on or off the theatre. This signals a Harmony Hub activity to power on the AV Amplifier and the projector.  Once the projector is powered up the lights are automatically dimmed to a comfortable lighting preset..
- When the media playback starts, the lighting is dimmed further to a preset that is almost entirely off.
- When media playback is paused the lighting is brought up a bit so people can navigate the room etc.
- When the user powers off the system via the remote control, the AV equipment is powered off, and the lighting is brought up to full.
- When the application will power down the system and lighting when there is no media playback happening and the room is vacant.
- The application implements watchdog timers to automatically power off the room after a configurable timeout.  This is used as a backup in case the occupancy sensor is inaccurate. (Projector bulbs are expensive! - do everything we can to make sure the projector isn't left on).
