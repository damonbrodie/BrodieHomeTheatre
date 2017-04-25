# Brodie Home Theatre

This project was developed to meet the needs for my [home theatre](http://www.avsforum.com/forum/19-dedicated-theater-design-construction/1033681-brodie-home-theatre-build-thread-2.html#post46048545) where I wanted very selective home automation to make the experience as seemless as possible.

The project incorporates the following technologies: 
 - Insteon light dimmers that provide a programmable lighting interface with a standard wall dimmer formfactor
 - Insteon motion sensor to detect room occupancy
 - Harmony Remote controls for media playback control, but also allow programmatic automation of certain functions
 - A beamforming array microphone for room scale "distant" voice recognition
 - Serial port integration to the Panasonic projector
 - Connection to the JSON port on the Kodi HTPC.  This is used to determine when media playback 
 pauses/plays/stops.  Additionally I read the entire media library so that I can use it for 
 voice recognition

This project forms an automation application that runs on the HTPC in the theatre that listens 
to the events and uses them as decision points to signal events.

## Use Cases
 
This project aims to provide very specific automation to both enhance the *cool* factor of the 
room as well as make it easier to use the theatre.  The following high level use cases are 
supported by this application:
- Upon entering the room (Insteon Motion Sensor/door sensor), bring up the room lights (Insteon 
Dimmer).
- Use speech recognition (Microsoft Speech Engine) to listen for voice requests to turn on or 
off the theatre. This signals a Harmony Hub activity to power on the AV Amplifier and the
projector.  Once the projector is powered up the lights are automatically dimmed to a comfortable 
lighting preset.
- The user can, using voice recognition, turn on the theatre "Turn on Theatre".  They can also 
say "Let's watch Star Wars".  In this example if the theatre isn't on, then it is powered on and 
then the media playback will begin.
- When the media playback starts (Kodi JSON), the lighting is dimmed further.
- Either with the Harmony remote or via voice recognition, the user can pause or stop media playback.
- When media playback is paused the lighting is brought up a bit so people can navigate the room etc.
- When the user powers off the system via the remote control, the AV equipment is powered off, and 
the lighting is brought up to full.
- The application will power down the system and lighting when there is no media playback happening 
and the room is vacant.
- The application implements a watchdog timer to automatically power off the room after a configurable 
timeout.  This is used as a backup in case the occupancy sensor is inaccurate. (Projector bulbs are 
expensive! - do everything we can to make sure the projector isn't left on).

## Speech Recognition
I've done a lot of experimentation with making the speech recognition as reliable as possible.  
Several different components make up this part of the solution:
 - Speech Recognition Engine.  There are several different speech recognition engines available.  
 Microsoft provide two of them.  One is "System.Speech", and this is available natively within Windows.  
 Additionally Microsoft has "Microsoft.Speech" and I've chosen to use that for this project.  System.Speech 
 works well for dictation and headset microphones - it is fully trainable on a per user basis.  Microsoft.Speech 
 isn't trainable, but it is specifically tuned for distant speech - it works best when you tell the speech 
 engine all of available phrases and it will listen only for those.
 - Microphone.  I'm using a beamforming microphone combined with a Sound Blaster Z sound card.  The card can 
 use the beam forming microphone to pick up speech from the seating area in front, and it does a reasonable 
 job filtering out the sounds that might be coming from the movie playback.  I've experimented with all
 of the microphone settings for Noise Cancellation, Focus Width and Microphone and Microphone boost levels 
 to come up with the combination that works best for my room setup.
 - Software.  Speech recognition is not perfect.  The recognition engine does it's best to identify when 
 it "hears" one of the key phrases.  Background conversations, the movie soundtrack, etc, are all supplying 
 stimulus that may be confusing to the engine.  To mitigate this, the application tries to be as contextually
 aware as possible.  If the room is vacant (no motion recorded by the motion sensor), stop listening for voice
 commands.  I the movie is playing, then limit the key phrases to just pause/stop playback.  Create lots of
 variations on the key phrases so it is natural to a wider audience:  "Let's watch the movie Rogue One" or
 "Play movie Rogue One", etc.

 Combining each of these facets of the approach has resulted in a surprisingly responsive voice recognition
 system.  Of course to limit user frustration there should be other manual ways to control the homer theatre
 (for example with a remote control).