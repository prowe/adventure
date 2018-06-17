import React, { Component } from 'react';
import Websocket from 'react-websocket'; 
import ReactMarkdown from 'react-markdown';

function TimelineEvent({event}) {
  return (
    <li key={event.id}><ReactMarkdown source={event.timelineMessage}/></li>
  );
}

class App extends Component {

  constructor(props) {
    super(props);
    this.state = {
      eventStream: []
    }
  }

  onMessage = (messageBody) => {
    console.log(`got message${messageBody}`);
    const event = JSON.parse(messageBody);
    if (event.timelineMessage) {
      this.pushTimelineEvent(event);
    }
  }

  pushTimelineEvent = (event) => {
    this.setState(({eventStream}) => {
      const newStream = [...eventStream, event].slice(-10);
      return {
        eventStream: newStream
      };
    });
  }

  render() {
    return (
      <div>
        Random content
        <ul>
          {this.state.eventStream.map(e => <TimelineEvent event={e}/>)}
        </ul>
        <Websocket url='ws://localhost:5000/event-stream'
            debug={true}
            onMessage={this.onMessage}/>
      </div>
    );
  }
}

export default App;
