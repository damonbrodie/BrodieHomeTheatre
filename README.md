# Brodie Home Theatre

This project was developed to meet the needs for my [home theatre](http://www.avsforum.com/forum/19-dedicated-theater-design-construction/1033681-brodie-home-theatre-build-thread-2.html#post46048545) where I wanted very selective home automation to make the experience as seemless as possible.

The project incorporates the following technologies: 
 - Insteon light dimmers that provide a programmable lighting interface with a standard wall dimmer formfactor
 - Insteon motion sensor to detect room occupancy
 - Harmony Remote controls for media playback control, but also allow programmatic automation of certain functions
 - Serial port integration to the Panasonic projector
 - Connection to the JSON port on the Kodi HTPC.  This is used to determine when media playback 
 pauses/plays/stops.

This project forms an automation application that runs on the HTPC in the theatre that listens 
to the events and uses them as decision points to signal events.

## Use Cases
 
This project aims to provide very specific automation to both enhance the *cool* factor of the 
room as well as make it easier to use the theatre.  The following high level use cases are 
supported by this application:
- Upon entering the room using the Insteon motion and door sensors, bring up the room lights with the Insteon 
Dimmer.
- Listen for Harmony Hub activity changes to power on the AV Amplifier. Use this to send serial commands to the
projector and power it on.  Once the projector is powered up the lights are automatically dimmed to a comfortable 
lighting preset.
- Using the Kodi JSON feed to stay current with playback status (pause/stop/start), alter the lighting, dimming
it further during playback, raising it during pause and then stopped.
- Listen for button presses from the Harmony: the user can pause/stop/start media playback as well as
raise/lower the room lighting.
- When the user powers off the system via the remote control, the AV equipment is powered off, and 
the lighting is brought up to full.
- The application will power down the system and lighting when there is no media playback happening 
and the room is vacant.
- The application implements a watchdog timer to automatically power off the room after a configurable 
timeout.  This is used as a backup in case the occupancy sensor is inaccurate. (Projector bulbs are 
expensive! - do everything we can to make sure the projector isn't left on).

## Speech Recognition

I tried to make voice recognition work with the off the shelf libraries from Microsoft.  The recognition just wasn't good
enough across the room to the microphone.  I tried several beam forming mics including the Kinect and a couple others.

I have removed all of that code from the solution and I am now using a Google Home mini.  I've setup a node.js server using this solution:

https://github.com/OmerTu/GoogleHomeKodi

Voice recognition is now working quite well.
