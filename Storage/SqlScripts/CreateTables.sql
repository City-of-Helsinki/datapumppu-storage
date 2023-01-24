
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
DECLARE exec_id uuid = '263ea07a-226d-4040-abfa-fe3f852a1719';
BEGIN
IF NOT EXISTS (SELECT id from database_updates WHERE id = exec_id) THEN

    ALTER TABLE agenda_items ADD COLUMN language VARCHAR(10);
    ALTER TABLE agenda_items DROP CONSTRAINT pk__agenda_items__meeting_id;
    ALTER TABLE agenda_items ADD CONSTRAINT pk__agenda_items__meeting_id UNIQUE (meeting_id, agenda_point, language);
    ALTER TABLE decisions ADD COLUMN language VARCHAR(10);

    INSERT INTO database_updates VALUES (exec_id);

end if;
end $$;

DO $$
DECLARE exec_id uuid = '1ae9e59f-70ef-407b-b89d-04f610ae7568';
BEGIN
IF NOT EXISTS (SELECT id from database_updates WHERE id = exec_id) THEN

    delete from decision_history_pdfs;
    delete from decision_pdfs;
    delete from decision_attachments;
    delete from decisions;
    
    INSERT INTO database_updates VALUES (exec_id);

end if;
end $$;

DO $$
DECLARE exec_id uuid = 'c586cd82-ea51-46a5-83a4-b37ba054366d';
BEGIN
IF NOT EXISTS (SELECT id from database_updates WHERE id = exec_id) THEN
    
    ALTER TABLE votings ALTER COLUMN for_text TYPE TEXT;
    ALTER TABLE votings ALTER COLUMN against_text TYPE TEXT;
    ALTER TABLE votings DROP CONSTRAINT pk__votings__voting_id CASCADE;
    ALTER TABLE votings RENAME COLUMN voting_id TO voting_number;
    ALTER TABLE votings ADD CONSTRAINT pk__votings__meeting_id__voting_number PRIMARY KEY (meeting_id, voting_number);
    ALTER TABLE votes ADD COLUMN meeting_id VARCHAR(64);
    ALTER TABLE votes RENAME COLUMN voting_id TO voting_number;
    ALTER TABLE votes ADD CONSTRAINT fk__votes__meeting_id__voting_id FOREIGN KEY (meeting_id, voting_number) REFERENCES votings(meeting_id, voting_number);

    INSERT INTO database_updates VALUES (exec_id);

end if;
end $$;

DO $$
DECLARE exec_id uuid = '8e6ae333-1cea-447e-87ab-354c6bdc07df';
BEGIN
IF NOT EXISTS (SELECT id from database_updates WHERE id = exec_id) THEN
    
    ALTER TABLE votings RENAME COLUMN voting_type_text TO voting_type_text_fi;
    ALTER TABLE votings ADD COLUMN voting_type_text_sv VARCHAR(64);
    ALTER TABLE votings RENAME COLUMN for_text TO for_text_fi;
    ALTER TABLE votings ADD COLUMN for_text_sv TEXT;
    ALTER TABLE votings RENAME COLUMN for_title TO for_title_fi;
    ALTER TABLE votings RENAME COLUMN against_title TO against_title_fi;
    ALTER TABLE votings ADD COLUMN for_title_sv VARCHAR(64);
    ALTER TABLE votings ADD COLUMN against_title_sv VARCHAR(64);
    ALTER TABLE votings RENAME COLUMN against_text TO against_text_fi;
    ALTER TABLE votings ADD COLUMN against_text_sv TEXT;
    ALTER TABLE meeting_seats RENAME COLUMN person_fi TO person;
    ALTER TABLE meeting_seats DROP COLUMN person_sv;
    ALTER TABLE meeting_seats ADD COLUMN additional_info_fi VARCHAR(64);
    ALTER TABLE meeting_seats ADD COLUMN additional_info_sv VARCHAR(64);
    ALTER TABLE propositions ALTER COLUMN text_fi TYPE TEXT;
    ALTER TABLE propositions ALTER COLUMN text_sv TYPE TEXT;

    INSERT INTO database_updates VALUES (exec_id);

end if;
end $$;

DO $$
DECLARE exec_id uuid = '0288404c-ff22-4876-b108-b852afcfcd67';
BEGIN
IF NOT EXISTS (SELECT id from database_updates WHERE id = exec_id) THEN

    ALTER TABLE speaking_turns RENAME COLUMN person_fi TO person;
    ALTER TABLE speaking_turns ADD COLUMN additional_info_fi VARCHAR(64);
    ALTER TABLE speaking_turns ADD COLUMN additional_info_sv VARCHAR(64);
    ALTER TABLE speaking_turns DROP COLUMN person_sv;

    INSERT INTO database_updates VALUES (exec_id);

end if;
end $$;

DO $$
DECLARE exec_id uuid = '394e0ee7-01c9-4876-b674-7e0b41951d08';
BEGIN
IF NOT EXISTS (SELECT id from database_updates WHERE id = exec_id) THEN


    CREATE TABLE agenda_item_attachments (
        meeting_id VARCHAR(64),
        agenda_point INT,
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
        CONSTRAINT fk__agenda_item_attachments__agenda_items__id FOREIGN KEY (meeting_id, agenda_point, language) REFERENCES agenda_items(meeting_id, agenda_point, language),
        CONSTRAINT pk__agenda_item_attachments__id PRIMARY KEY (meeting_id, agenda_point, attachment_number)
     );

 
    CREATE TABLE agenda_item_pdfs (
        meeting_id VARCHAR(64),
        agenda_point INT,
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
        CONSTRAINT fk__agenda_item_pdfs__agenda_point__agenda_items__id FOREIGN KEY (meeting_id, agenda_point, language) REFERENCES agenda_items(meeting_id, agenda_point, language),
        CONSTRAINT pk__agenda_item_pdfs__id PRIMARY KEY (meeting_id, agenda_point)
    );


    CREATE TABLE agenda_item_decision_history_pdfs (
        meeting_id VARCHAR(64),
        agenda_point INT,
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
        CONSTRAINT fk__agenda_item_decision_history_pdfs__agenda_point__agenda_items__id FOREIGN KEY (meeting_id, agenda_point, language) REFERENCES agenda_items(meeting_id, agenda_point, language),
        CONSTRAINT pk__agenda_item_decision_history_pdfs__id PRIMARY KEY (meeting_id, agenda_point)
    );

    INSERT INTO database_updates VALUES (exec_id);

end if;
end $$;


DO $$
DECLARE exec_id uuid = '39774ea0-5558-4238-a3cb-8bc9a9d08d02';
BEGIN
IF NOT EXISTS (SELECT id from database_updates WHERE id = exec_id) THEN

    CREATE TABLE admin_users (
        username varchar (256),
        password varchar (64),
        CONSTRAINT pk__admin_users__username PRIMARY KEY (username)
    );

    INSERT INTO database_updates VALUES (exec_id);

end if;
end $$;


DO $$
DECLARE exec_id uuid = '6c4d9600-141b-4c2b-b58c-b158a9443d3b';
BEGIN
IF NOT EXISTS (SELECT id from database_updates WHERE id = exec_id) THEN

    CREATE TABLE video_synchronizations(
        meeting_id VARCHAR(64),
        CONSTRAINT fk__video_synchronization__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id),
        timestamp TIMESTAMP,
        video_position INT,
        CONSTRAINt pk__video_synchronizations__meeting_id PRIMARY KEY (meeting_id, video_position)
    );

    INSERT INTO database_updates VALUES (exec_id);

end if;
end $$;


DO $$
DECLARE exec_id uuid = 'f7ea741d-12e9-45f1-910c-43abf477a446';
BEGIN
IF NOT EXISTS (SELECT id from database_updates WHERE id = exec_id) THEN

    ALTER TABLE cases RENAME COLUMN case_text TO case_text_fi;
    ALTER TABLE cases ALTER COLUMN case_text_fi TYPE TEXT;
    ALTER TABLE cases ADD COLUMN case_text_sv TEXT;

    ALTER TABLE cases RENAME COLUMN item_text TO item_text_fi;
    ALTER TABLE cases ALTER COLUMN item_text_fi TYPE TEXT;
    ALTER TABLE cases ADD COLUMN item_text_sv TEXT;

    ALTER TABLE cases ADD COLUMN case_number VARCHAR(10);
    ALTER TABLE cases ADD COLUMN item_number VARCHAR(10);

    ALTER TABLE cases RENAME COLUMN case_id TO identifier;
    ALTER TABLE cases DROP CONSTRAINT pk__cases__meeting_id;
    ALTER TABLE cases ALTER COLUMN identifier DROP NOT NULL;

    ALTER TABLE cases ADD CONSTRAINT pk__cases__meeting_id PRIMARY KEY (meeting_id, case_number, item_number);

    INSERT INTO database_updates VALUES (exec_id);

end if;
end $$;

DO $$
DECLARE exec_id uuid = 'e4febf6a-dc1b-4a1f-80bb-90915b0f058f';
BEGIN
IF NOT EXISTS (SELECT id from database_updates WHERE id = exec_id) THEN

    ALTER TABLE cases ALTER COLUMN proposition_fi TYPE TEXT;
    ALTER TABLE cases ALTER COLUMN proposition_sv TYPE TEXT;
    ALTER TABLE meeting_events ALTER COLUMN sequence_number TYPE BIGINT;
    ALTER TABLE meeting_seat_updates ALTER COLUMN sequence_number TYPE BIGINT;

    INSERT INTO database_updates VALUES (exec_id);

end if;
end $$;


DO $$
DECLARE exec_id uuid = 'a420c082-38ff-4f8c-b8d4-c02b775e7213';
BEGIN
IF NOT EXISTS (SELECT id from database_updates WHERE id = exec_id) THEN

    ALTER TABLE speech_timer_events RENAME COLUMN person_fi TO person;
    ALTER TABLE speech_timer_events DROP COLUMN person_sv;
    ALTER TABLE speech_timer_events ADD COLUMN additional_info_fi VARCHAR(64);
    ALTER TABLE speech_timer_events ADD COLUMN additional_info_sv VARCHAR(64);

    ALTER TABLE propositions RENAME COLUMN person_fi TO person;
    ALTER TABLE propositions DROP COLUMN person_sv;
    ALTER TABLE propositions ADD COLUMN additional_info_fi VARCHAR(64);
    ALTER TABLE propositions ADD COLUMN additional_info_sv VARCHAR(64);

    ALTER TABLE person_events RENAME COLUMN person_fi TO person;
    ALTER TABLE person_events DROP COLUMN person_sv;
    ALTER TABLE person_events ADD COLUMN additional_info_fi VARCHAR(64);
    ALTER TABLE person_events ADD COLUMN additional_info_sv VARCHAR(64);    

    ALTER TABLE speaking_turn_reservations RENAME COLUMN person_fi TO person;
    ALTER TABLE speaking_turn_reservations DROP COLUMN person_sv;
    ALTER TABLE speaking_turn_reservations ADD COLUMN additional_info_fi VARCHAR(64);
    ALTER TABLE speaking_turn_reservations ADD COLUMN additional_info_sv VARCHAR(64);    

    ALTER TABLE started_speaking_turns RENAME COLUMN person_fi TO person;
    ALTER TABLE started_speaking_turns DROP COLUMN person_sv;
    ALTER TABLE started_speaking_turns ADD COLUMN additional_info_fi VARCHAR(64);
    ALTER TABLE started_speaking_turns ADD COLUMN additional_info_sv VARCHAR(64);    

    ALTER TABLE reply_reservations RENAME COLUMN person_fi TO person;
    ALTER TABLE reply_reservations DROP COLUMN person_sv;
    ALTER TABLE reply_reservations ADD COLUMN additional_info_fi VARCHAR(64);
    ALTER TABLE reply_reservations ADD COLUMN additional_info_sv VARCHAR(64);   
    
    ALTER TABLE votes RENAME COLUMN voter_name TO person;
    ALTER TABLE votes ADD COLUMN additional_info_fi VARCHAR(64);
    ALTER TABLE votes ADD COLUMN additional_info_sv VARCHAR(64);

    INSERT INTO database_updates VALUES (exec_id);

end if;
end $$;

DO $$
DECLARE exec_id uuid = 'b0125a25-4780-4211-8846-00f2811fc4cc';
BEGIN
IF NOT EXISTS (SELECT id from database_updates WHERE id = exec_id) THEN


    CREATE INDEX ix__video_synchronizations__timestamp  ON video_synchronizations (timestamp);

    INSERT INTO database_updates VALUES (exec_id);

end if;
end $$;


DO $$
DECLARE exec_id uuid = 'cc852698-6376-4f7b-b227-6ad5d069a520';
BEGIN
IF NOT EXISTS (SELECT id from database_updates WHERE id = exec_id) THEN

    ALTER TABLE speaking_turns RENAME TO statements;
    ALTER TABLE speaking_turn_reservations RENAME TO statement_reservations;
    ALTER TABLE started_speaking_turns RENAME TO started_statements;
    ALTER TABLE break_notices RENAME TO pause_infos;

    INSERT INTO database_updates VALUES (exec_id);

end if;
end $$;


DO $$
DECLARE exec_id uuid = 'ea3133b1-08d3-454e-b402-c685aa555414';
BEGIN
IF NOT EXISTS (SELECT id from database_updates WHERE id = exec_id) THEN

    ALTER TABLE agenda_items DROP CONSTRAINT fk__agenda_items__meeting_id__meetings__meeting_id;
    ALTER TABLE agenda_items ADD CONSTRAINT fk__agenda_items__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id) ON DELETE CASCADE;

    ALTER TABLE meeting_events DROP CONSTRAINT fk__meeting_events__meeting_id__meetings__meeting_id;
    ALTER TABLE meeting_events ADD CONSTRAINT fk__meeting_events__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id) ON DELETE CASCADE;

    ALTER TABLE votings DROP CONSTRAINT fk__votings__meeting_id__meetings__meeting_id;
    ALTER TABLE votings ADD CONSTRAINT fk__votings__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id) ON DELETE CASCADE;

    ALTER TABLE decisions DROP CONSTRAINT fk__decisions__meeting_id__meetings__meeting_id;
    ALTER TABLE decisions ADD CONSTRAINT fk__decisions__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id) ON DELETE CASCADE;

    ALTER TABLE meeting_seat_updates DROP CONSTRAINT fk__meeting_seat_updates__meeting_id__meetings__meeting_id;
    ALTER TABLE meeting_seat_updates ADD CONSTRAINT fk__meeting_seat_updates__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id) ON DELETE CASCADE;

    ALTER TABLE statements DROP CONSTRAINT fk__speaking_turns__meeting_id__meetings__meeting_id;
    ALTER TABLE statements ADD CONSTRAINT fk__statements__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id) ON DELETE CASCADE;
    ALTER TABLE statements DROP CONSTRAINT fk__speaking_turns__event_id__meeting_events__event_id;
    ALTER TABLE statements ADD CONSTRAINT fk__statements__event_id__meeting_events__event_id FOREIGN KEY (event_id) REFERENCES meeting_events(event_id) ON DELETE CASCADE;

    ALTER TABLE cases DROP CONSTRAINT fk__cases__meeting_id__meetings__meeting_id;
    ALTER TABLE cases ADD CONSTRAINT fk__cases__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id) ON DELETE CASCADE;

    ALTER TABLE roll_calls DROP CONSTRAINT fk__roll_calls__meeting_id__meetings__meeting_id;
    ALTER TABLE roll_calls ADD CONSTRAINT fk__roll_calls__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id) ON DELETE CASCADE;

    ALTER TABLE statement_reservations DROP CONSTRAINT fk__speaking_turn_reservations__meeting_id__meetings__meeting_id;
    ALTER TABLE statement_reservations ADD CONSTRAINT fk__statement_reservations__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id) ON DELETE CASCADE;
    ALTER TABLE statement_reservations DROP CONSTRAINT fk__speaking_turn_reservations__event_id__meeting_events__event_id ;
    ALTER TABLE statement_reservations ADD CONSTRAINT fk__statement_reservations__event_id__meeting_events__event_id FOREIGN KEY (event_id) REFERENCES meeting_events(event_id) ON DELETE CASCADE;

    ALTER TABLE started_statements DROP CONSTRAINT fk__started_speaking_turns__meeting_id__meetings__meeting_id;
    ALTER TABLE started_statements ADD CONSTRAINT fk__started_statements__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id) ON DELETE CASCADE;
    ALTER TABLE started_statements DROP CONSTRAINT fk__started_speaking_turns__event_id__meeting_events__event_id ;
    ALTER TABLE started_statements ADD CONSTRAINT fk__started_statements__event_id__meeting_events__event_id FOREIGN KEY (event_id) REFERENCES meeting_events(event_id) ON DELETE CASCADE;

    ALTER TABLE person_events DROP CONSTRAINT fk__person_events__meeting_id__meetings__meeting_id;
    ALTER TABLE person_events ADD CONSTRAINT fk__person_events__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id) ON DELETE CASCADE;

    ALTER TABLE pause_infos DROP CONSTRAINT fk__break_notices__meeting_id__meetings__meeting_id;
    ALTER TABLE pause_infos ADD CONSTRAINT fk__pause_infos__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id) ON DELETE CASCADE;
    ALTER TABLE pause_infos DROP CONSTRAINT fk__break_notices__event_id__meeting_events__event_id;
    ALTER TABLE pause_infos ADD CONSTRAINT fk__pause_infos__event_id__meeting_events__event_id FOREIGN KEY (event_id) REFERENCES meeting_events(event_id) ON DELETE CASCADE;

    ALTER TABLE speech_timer_events DROP CONSTRAINT fk__speech_timer_events__meeting_id__meetings__meeting_id;
    ALTER TABLE speech_timer_events ADD CONSTRAINT fk__speech_timer_events__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id) ON DELETE CASCADE;

    ALTER TABLE propositions DROP CONSTRAINT fk__propositions__meeting_id__meetings__meeting_id;
    ALTER TABLE propositions ADD CONSTRAINT fk__propositions__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id) ON DELETE CASCADE;

    ALTER TABLE reply_reservations DROP CONSTRAINT fk__reply_reservations__meeting_id__meetings__meeting_id;
    ALTER TABLE reply_reservations ADD CONSTRAINT fk__reply_reservations__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id) ON DELETE CASCADE;

    ALTER TABLE video_synchronizations DROP CONSTRAINT fk__video_synchronization__meeting_id__meetings__meeting_id;
    ALTER TABLE video_synchronizations ADD CONSTRAINT fk__video_synchronization__meeting_id__meetings__meeting_id FOREIGN KEY (meeting_id) REFERENCES meetings(meeting_id) ON DELETE CASCADE;

    INSERT INTO database_updates VALUES (exec_id);

end if;
end $$;
