import React from 'react';
import styles from './LoadingMessage.css';

const messages = [
  'Did somebody order a pizza?',
  'Is that a rocket in your pocket, or are you just happy to see me?',
  'There are actually tons of hot singles in your area, but none of them are interested in you.',
  'She\'s going to be really pissed when she finds out.',
  'What are you doing step bro?',
  'I hope the lemon stealing whores don\'t steal any of our lemons.',
  'But I poop from there! Not right now you don\'t',
  'Wait. Why is there a dick in me?',
  'It\'s cock porn! I mean pop corn, I\'m dyslexic.',
  'Is that dirty enough for you?',
  'Last night I dreamed my cock was a three-masted Spanish galleon.',
  'I was fisted once',
  'Why don\'t you have a seat right over there.',
  'Right up your chocolate bon-bon factory',
  'Wait. Hold up. Bring that camera over here a second. Look. Just look at this. She\'s only gone and started without me.',
  'What am I even here for? I mean what am I, just a piece of meat or something?',
  'Hup! There it is!!',
  'Shoes for traction!',
  'Holy ejaculations Batman!',
  'I wanna glaze you like a donut',
  'Oh yeah baby, keep me open like 7/11.',
  'The difference between pornography and erotica is lighting',
  'I once stole a pornographic book that was printed in Braille; I used to rub the dirty parts.',
  'I reckon porn gives kids an unrealistic idea of what it\'s like to be a plumber.',
  'The couple next door have just made a sex tape… obviously, they don\'t know that yet.',
  'There\.s nothing but porn on TV these days. I tell you, it makes me so angry, I sit on the end of my bed and shake my fist at it.',
  'Hi, I\'ve come to clean your pool',
  'Why do you think the net was born? Porn! Porn! Porn!',
  'All these guys unzip their flies for... Porn! Porn! Porn!',
  'Grab your dick and double click...',
  'Thank you for your cervix.',
  'Look I can talk or I can suck your cock, but I\'m not a fucking ventriloquist.',
  'Ohhh yea girl, get the happy meal.',
  'Keep flapping, Nigel!',
  'Step out of the car, miss. I need to check your cavity.',
  'Wait a minute, this isnt Super Smash Bros. This is anal sex.',
  'You can\'t tempt me with with your wares.',
  'Is it going to hurt?',
  'I want my pussy inside of you',
  'Take it like a man',
  'Hey buddy I think you got the wrong door, the leather club\'s two blocks down.',
  'Hey, has it been about ten seconds since we looked at our lemon tree?',
  'Look it\'s dick o\'clock!',
  'My brain knows you\'re full of shit but my body wants to fuck you.',
  'I\'m not really a wolf... I\'m Skeletor! Hahaha',
  'If you were a lemon, I would put you on my shelf and cherish you like I cherish all our lemons',
  'Im gonna bury my dick so far inside your ass whoever could pull it out would be crowned King Arthur',
  'The only thing that separates us from the animals…is we have pornography.',
  'There is no dignity when the human dimension is eliminated from the person. In short, the problem with pornography is not that it shows too much of the person, but that it shows far too little.',
  'It is difficult to describe how it feels to gaze at living human beings whom you\'ve seen perform in hard-core porn. To shake the hand of a man whose precise erectile size, angle, and vasculature are known to you….To have seen these strangers\' faces in orgasm — that most unguarded and purely neural of expressions, the one so vulnerable that for centuries you basically had to marry a person to get to see it.',
  'Any violation of a woman\'s body can become sex for men; this is the essential truth of pornography.',
  'Legend has it that every new technology is first used for something related to sex or pornography. That seems to be the way of humankind.',
  'Unzipping...',
  'Uncensoring...',
];

let message = null;

function LoadingMessage() {
  if (!message) {
    const index = Math.floor(Math.random() * messages.length);
    message = messages[index];
  }

  return (
    <div className={styles.loadingMessage}>
      {message}
    </div>
  );
}

export default LoadingMessage;
