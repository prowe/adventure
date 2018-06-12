import React, { Component } from 'react';
import Websocket from 'react-websocket'; 
import ReactMarkdown from 'react-markdown';

function TimelineEvent({event}) {
  return (
    <li key={event.id}><ReactMarkdown source={event.message}/></li>
  );
}

class App extends Component {

  constructor(props) {
    super(props);
    this.state = {
      eventStream: []
    }
  }

  onMessage = (message) => {
    console.log('got message', message);
    this.setState(({eventStream}) => {
      const event = {
        id: Date.now(),
        message
      };
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
            onMessage={this.onMessage}/>
      </div>
    );
  }
}

export default App;
