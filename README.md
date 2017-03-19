# Brodie Home Theatre

This project was developed to meet the needs for my [home theatre](http://www.avsforum.com/forum/19-dedicated-theater-design-construction/1033681-brodie-home-theatre-build-thread-2.html#post46048545) where I wanted very selective home automation to make the experience as seemless as possible.

The project incorporates the following technologies: 
 - Insteon light dimmers that provide a programmable lighting interface with a standard wall dimmer formfactor
 - Insteon motion sensor to detect room occupancy
 - Harmony Remote controls for media playback control, but also allow programmatic automation of certain functions
 - A beamforming array microphone for room scale "distant" voice recognition
 - The brain of my theatre is an HTPC running Kodi where I do all of my media playback.  Kodi has a plugin framework and I use that to understand the state of the media player to the automation framework (this project).

This project forms an automation application that runs on the HTPC in the theatre that listens to the events and uses them as decision points to signal events.

## Use Cases
 
This project aims to provide very specific automation to both enhance the *cool* factor of the room as well as make it easier to use the theatre.  The following high level use cases are supported by this application:
- Upon entering the room (Insteon Motion Sensor), bring up the room lights (Insteon Dimmer).
- Use speech recognition (Microsoft Speech Engine) to listen for voice requests to turn on or off the theatre. This signals a Harmony Hub activity to power on the AV Amplifier and the projector.  Once the projector is powered up the lights are automatically dimmed to a comfortable lighting preset.
- When the media playback starts (Kodi Plugin), the lighting is dimmed further.
- When media playback is paused the lighting is brought up a bit so people can navigate the room etc.
- When the user powers off the system via the remote control, the AV equipment is powered off, and the lighting is brought up to full.
- The application will power down the system and lighting when there is no media playback happening and the room is vacant.
- The application implements awatchdog timer to automatically power off the room after a configurable timeout.  This is used as a backup in case the occupancy sensor is inaccurate. (Projector bulbs are expensive! - do everything we can to make sure the projector isn't left on).
