
create table if not exists database_updates (
	id UUID,
	CONSTRAINT pk__database_updates__id PRIMARY KEY (id)
);

DO $$
DECLARE exec_id uuid = 'c75d4e52-1625-4dea-9090-dcb965f18ca3';
BEGIN
IF NOT EXISTS (SELECT id from database_updates WHERE id = exec_id) THEN

    insert into database_updates values (exec_id);

    CREATE TABLE meetings (
        meeting_id VARCHAR(64),
        name VARCHAR(256),
        meeting_date TIMESTAMP,
        location VARCHAR(256),
        meeting_title_fi VARCHAR(512),
        meeting_title_sv VARCHAR(512),
        meeting_started TIMESTAMP,
        meeting_started_eventid UUID,
        meeting_ended TIMESTAMP,
        meeting_ended_eventid UUID,
        CONSTRAINT pk__meetings__meeting_id PRIMARY KEY (meeting_id)
    );

    CREATE INDEX meetings_started_ended_idx 
    ON meetings (meeting_started, meeting_ended);

    CREATE TABLE agenda_items (
	    meeting_id VARCHAR(64),
        CONSTRAINT fk__agenda_items__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id),
        agenda_point INT,
        section VARCHAR(64),
        title VARCHAR(512),
        case_id_label VARCHAR(64),
        html_content TEXT,
        html_decision_history TEXT,
        CONSTRAINT pk__agenda_items__meeting_id PRIMARY KEY (meeting_id, agenda_point, title)
    );

    CREATE INDEX agenda_items_meeting_id_agenda_point_idx
    ON agenda_items (meeting_id, agenda_point);

    CREATE TABLE meeting_events (
	    meeting_id VARCHAR(64),
        CONSTRAINT fk__meeting_events__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id),
        event_id UUID,
        event_type VARCHAR(64),
        timestamp TIMESTAMP,
        sequence_number INT,
        case_number VARCHAR(10),
        item_number VARCHAR(10),
        CONSTRAINT pk__meeting_events__event_id PRIMARY KEY(event_id)
    );

    CREATE TABLE votings (
	    meeting_id VARCHAR(64),
        CONSTRAINT fk__votings__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id),
        voting_id INT,
        voting_type INT,
        voting_type_text VARCHAR(64),
        voting_started TIMESTAMP,
	    voting_started_eventid UUID,
        CONSTRAINT fk__votings__voting_started_eventid__meeting_events__event_id FOREIGN KEY (voting_started_eventid) REFERENCES meeting_events(event_id),
        voting_ended TIMESTAMP,
	    voting_ended_eventid UUID,
        CONSTRAINT fk__votings__voting_ended_eventid__meeting_events__event_id  FOREIGN KEY (voting_ended_eventid) REFERENCES meeting_events(event_id),
        votes_for INT,
        votes_against INT,
        votes_empty INT,
        votes_absent INT,
        for_text VARCHAR(64),
        for_title VARCHAR(64),
        against_text VARCHAR(64),
        against_title VARCHAR(64),
        CONSTRAINT pk__votings__voting_id PRIMARY KEY (voting_id)
    );

    CREATE TABLE votes (
        voting_id INT, 
	    CONSTRAINT fk__votes__voting_id__votings__voting_id FOREIGN KEY (voting_id) REFERENCES votings(voting_id),
        vote_id INT GENERATED ALWAYS AS IDENTITY,
        voter_name VARCHAR(64),
        vote_type INT,
        CONSTRAINT pk__votes__vote_id primary key (vote_id)
    );

    CREATE TABLE decisions (
        native_id VARCHAR(64),
        title VARCHAR(512),
        case_id_label VARCHAR(64),
        case_id VARCHAR(64),
        section VARCHAR(10),
        html TEXT,
        history_html TEXT,
        motion TEXT,
        classification_code VARCHAR(128),
        classification_title VARCHAR(128),
        CONSTRAINT pk__decisions__native_id PRIMARY KEY (native_id)
    );

    CREATE TABLE decision_attachments (
        decision_id VARCHAR(64),
	    CONSTRAINT fk__decision_attachments__decision_id__decisions__native_id FOREIGN KEY (decision_id) REFERENCES decisions(native_id),
        native_id VARCHAR(64),
        title VARCHAR(256),
        attachment_number VARCHAR(64),
        publicity_class VARCHAR(32),
        security_reasons VARCHAR(128),
        type VARCHAR(64),
        file_uri VARCHAR(256),
        language VARCHAR(10),
        personal_data VARCHAR(128),
        issued VARCHAR(64),
        CONSTRAINT pk__decision_attachments__decision_id PRIMARY KEY (decision_id, attachment_number)
    );



    CREATE TABLE decision_pdfs (
        decision_id VARCHAR(64),
	    CONSTRAINT fk__decision_pdfs__decision_id__decisions__native_id FOREIGN KEY (decision_id) REFERENCES decisions(native_id),
        native_id VARCHAR(64),
        title VARCHAR(256),
        attachment_number VARCHAR(64),
        publicity_class VARCHAR(32),
        security_reasons VARCHAR(128),
        type VARCHAR(64),
        file_uri VARCHAR(256),
        language VARCHAR(10),
        personal_data VARCHAR(128),
        issued VARCHAR(64),
        CONSTRAINT pk__decision_pdfs__decision_id PRIMARY KEY (decision_id)
    );

    CREATE TABLE decision_history_pdfs (
        decision_id VARCHAR(64),
	    constraint fk__decision_history_pdfs__decision_id__decisions__native_id FOREIGN KEY (decision_id) REFERENCES decisions(native_id),
        native_id VARCHAR(64),
        title VARCHAR(256),
        attachment_number VARCHAR(64),
        publicity_class VARCHAR(32),
        security_reasons VARCHAR(128),
        type VARCHAR(64),
        file_uri VARCHAR(256),
        language VARCHAR(10),
        personal_data VARCHAR(128),
        issued VARCHAR(64),
        CONSTRAINT pk__decision_history_pdfs__decision_id PRIMARY KEY (decision_id)
    );

    CREATE TABLE meeting_seat_updates (
        meeting_id VARCHAR(64),
	    CONSTRAINT fk__meeting_seat_updates__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id),
        attendees_eventid UUID,
	    CONSTRAINT fk__meeting_seat_updates__attendees_eventid__meeting_events__event_id FOREIGN KEY (attendees_eventid) REFERENCES meeting_events(event_id),
        id INT GENERATED ALWAYS AS IDENTITY,
        sequence_number INT, 
        timestamp TIMESTAMP,
        CONSTRAINT pk__meeting_seat_updates__id PRIMARY KEY (id)
    );

    CREATE TABLE meeting_seats (
        meeting_seat_update_id INT,
	    CONSTRAINT fk__meeting_seats__meeting_seat_update_id__meeting_seat_updates__id FOREIGN KEY (meeting_seat_update_id) REFERENCES meeting_seat_updates(id),
        seat_id VARCHAR(64),
        person_fi VARCHAR(64),
        person_sv VARCHAR(64)
    );

    CREATE TABLE speaking_turns (
        meeting_id VARCHAR(64),
	    CONSTRAINT fk__speaking_turns__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id),
        event_id UUID,
	    CONSTRAINT fk__speaking_turns__event_id__meeting_events__event_id FOREIGN KEY (event_id) REFERENCES meeting_events(event_id),
        person_fi VARCHAR(64),
        person_sv VARCHAR(64),
        started TIMESTAMP,
        ended TIMESTAMP,
        speech_type INT,
        duration_seconds INT,
        CONSTRAINT pk__speaking_turns__meeting_id primary key (meeting_id, started)
    );

    CREATE TABLE cases (
        meeting_id VARCHAR(64),
	    CONSTRAINT fk__cases__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id),
        event_id UUID,
	    CONSTRAINT fk__cases__event_id__meeting_events__event_id FOREIGN KEY (event_id) REFERENCES meeting_events(event_id),
        proposition_fi VARCHAR(512),
        proposition_sv VARCHAR(512),
        case_text VARCHAR(512),
        item_text VARCHAR(512),
        case_id VARCHAR(64),
        CONSTRAINT pk__cases__meeting_id PRIMARY KEY (meeting_id, case_id)
    );

    CREATE TABLE roll_calls (
        meeting_id VARCHAR(64),
	    CONSTRAINT fk__roll_calls__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id),
        roll_call_started TIMESTAMP,
        roll_call_started_eventid UUID,
	    CONSTRAINT fk__roll_calls__roll_call_started_eventid__meeting_events__event_id FOREIGN KEY (roll_call_started_eventid) REFERENCES meeting_events(event_id),
        roll_call_ended TIMESTAMP,
        roll_call_ended_eventid UUID,
	    CONSTRAINT fk__roll_calls__roll_call_ended_eventid__meeting_events__event_id FOREIGN KEY (roll_call_ended_eventid) REFERENCES meeting_events(event_id),
        present INT,
        absent INT,
        CONSTRAINT pk__roll_calls__meeting_id PRIMARY KEY (meeting_id)
    );

    CREATE TABLE speaking_turn_reservations (
        meeting_id VARCHAR(64),
	    CONSTRAINT fk__speaking_turn_reservations__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id),
        event_id UUID,
	    CONSTRAINT fk__speaking_turn_reservations__event_id__meeting_events__event_id FOREIGN KEY (event_id) REFERENCES meeting_events(event_id),
        timestamp TIMESTAMP,
        person_fi VARCHAR(64),
        person_sv VARCHAR(64),
        ordinal INT,
        seat_id VARCHAR(5)
    );

    CREATE TABLE started_speaking_turns (
        meeting_id VARCHAR(64),
	    CONSTRAINT fk__started_speaking_turns__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id),
        event_id UUID,
	    CONSTRAINT fk__started_speaking_turns__event_id__meeting_events__event_id FOREIGN KEY (event_id) REFERENCES meeting_events(event_id),
        timestamp TIMESTAMP,
        person_fi VARCHAR(64),
        person_sv VARCHAR(64),
        speaking_time INT,
        speech_timer INT,
        start_time TIMESTAMP,
        direction VARCHAR(32),
        seat_id VARCHAR(64),
        speech_type INT
    );

    CREATE TABLE person_events (
        meeting_id VARCHAR(64),
	    CONSTRAINT fk__person_events__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id),
        event_id UUID,
	    CONSTRAINT fk__person_events__event_id__meeting_events__event_id FOREIGN KEY (event_id) REFERENCES meeting_events(event_id),
        timestamp TIMESTAMP,
        person_fi VARCHAR(64),
        person_sv VARCHAR(64),
        event_type INT,
        seat_id VARCHAR(64)
    );

    CREATE TABLE break_notices (
        meeting_id VARCHAR(64),
	    CONSTRAINT fk__break_notices__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id),
        event_id UUID,
	    CONSTRAINT fk__break_notices__event_id__meeting_events__event_id FOREIGN KEY (event_id) REFERENCES meeting_events(event_id),
        notice TEXT
    );

    CREATE TABLE speech_timer_events (
        meeting_id VARCHAR(64),
	    CONSTRAINT fk__speech_timer_events__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id),
        event_id UUID,
	    CONSTRAINT fk__speech_timer_events__event_id__meeting_events__event_id FOREIGN KEY (event_id) REFERENCES meeting_events(event_id),
        seat_id VARCHAR(64),
        person_fi VARCHAR(64),
        person_sv VARCHAR(64),
        duration_seconds INT,
        speech_timer INT,
        direction VARCHAR(64)
    );

    CREATE TABLE propositions (
        meeting_id VARCHAR(64),
	    CONSTRAINT fk__propositions__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id),
        event_id UUID,
	    CONSTRAINT fk__propositions__event_id__meeting_events__event_id FOREIGN KEY (event_id) REFERENCES meeting_events(event_id),
        text_fi VARCHAR(512),
        text_sv VARCHAR(512),
        person_fi VARCHAR(64),
        person_sv VARCHAR(64),
        type VARCHAR(64),
        type_text_fi VARCHAR(64),
        type_text_sv VARCHAR(64)
    );
end if;
end $$;

DO $$
DECLARE exec_id uuid = '39571e37-f506-4c98-9cd4-0c8b88bd30b4';
BEGIN
IF NOT EXISTS (SELECT id from database_updates WHERE id = exec_id) THEN

    insert into database_updates values (exec_id);

    CREATE TABLE reply_reservations (
        meeting_id VARCHAR(64),
	    CONSTRAINT fk__reply_reservations__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id),
        event_id UUID,
	    CONSTRAINT fk__reply_reservations__event_id__meeting_events__event_id FOREIGN KEY (event_id) REFERENCES meeting_events(event_id),
        person_fi VARCHAR(64),
        person_sv VARCHAR(64)
    );
end if;
end $$;


DO $$
DECLARE exec_id uuid = 'b051c198-a012-4b15-bb67-06072f317793';
BEGIN
IF NOT EXISTS (SELECT id from database_updates WHERE id = exec_id) THEN

    ALTER TABLE meetings ADD COLUMN meeting_sequence_number INT;
    ALTER TABLE decisions ADD COLUMN meeting_id VARCHAR(64);
    ALTER TABLE decisions ADD CONSTRAINT fk__decisions__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id);
    INSERT INTO database_updates VALUES (exec_id);

end if;
end $$;

DO $$
DECLARE exec_id uuid = 'b8c53363-39db-47bc-85bc-e507faa44f66';
BEGIN
IF NOT EXISTS (SELECT id from database_updates WHERE id = exec_id) THEN

    ALTER TABLE agenda_items ADD COLUMN language VARCHAR(10);
    ALTER TABLE agenda_items DROP CONSTRAINT pk__agenda_items__meeting_id;
    ALTER TABLE agenda_items ADD CONSTRAINT pk__agenda_items__meeting_id UNIQUE (meeting_id, agenda_point, language);
    INSERT INTO database_updates VALUES (exec_id);

end if;
end $$;